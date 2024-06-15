using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Helious.Utils;
using HID_API;

namespace Helious.Modules;

public class ServerHandler
{
    private readonly bool _hasMouse;
    private readonly List<long> _timingR = new();
    private readonly List<long> _timingS = new();

    public ServerHandler(HidHandler hidHandler, IPAddress hostAddress)
    {
        while (true)
        {
            if (!_hasMouse)
            {
                if (hidHandler.HidMouseHandlers.Count > 0)
                {
                    _hasMouse = true;
                    break;
                }

                Thread.Sleep(1);
            }
        }
    
        var localEndpoint = new IPEndPoint(hostAddress, 7483);
        using Socket listener = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        listener.Blocking = false;
        listener.Bind(localEndpoint);

        Span<byte> bytes = GC.AllocateArray<byte>(8, true);
        Stopwatch sending = new Stopwatch();
        Stopwatch reading = new Stopwatch();
        
        var hidStream = hidHandler.CreateHidStream("/dev/hidg1");
        
        while (true)
        {
            if (listener.Available != 0)
            {
                var received = listener.Receive(bytes);

                if (received != 0)
                {
                    short centerX = (short) (bytes[0] | bytes[1] << 8);
                    short centerY = (short) (bytes[2] | bytes[3] << 8);
                    var ignoreAim = bytes.Length > 4 && bytes[4] > 0;

                    bool? left = bytes.Length > 5 ? bytes[5] > 0 ? true : null : null;
                    bool? right = bytes.Length > 6 ? bytes[6] > 0 ? true : null : null;
                    bool? middle = bytes.Length > 7 ? bytes[7] > 0 ? true : null : null;

                    reading.Restart();
                    var mouseState = hidHandler.HidMouseHandlers[0].GetMouseState();
                    reading.Stop();
                    
                    sending.Restart();
                    if (mouseState.RightButton | ignoreAim)
                    {
                        left ??= mouseState.LeftButton;
                        right ??= mouseState.RightButton;
                        middle ??= mouseState.MiddleButton;
                    
                        hidHandler.WriteMouseReport(mouseState with
                        {
                            LeftButton = (bool) left,
                            RightButton = (bool) right,
                            MiddleButton = (bool) middle,
                            X = centerX,
                            Y = centerY,
                            Wheel = 0
                        }, hidStream);
                    }
                    sending.Stop();
                    
                    _timingR.Add(reading.ElapsedTicks);
                    _timingS.Add(sending.ElapsedTicks);

                    if (_timingR.Count != 0 && _timingS.Count != 0 && _timingR.Count % 100 == 0)
                    {
                        ConsoleUtils.WriteLine($"Reading: {_timingR.Average() / 10000.0}, Sending: {_timingS.Average() / 10000.0}");
                    }
                }
            }
        }
    }
}