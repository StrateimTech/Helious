using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using Helious.Utils;
using HID_API;

namespace Helious.Game;

public class ServerHandler
{
    private readonly bool _hasMouse;
    private readonly EndPoint _localEndpoint = new IPEndPoint(IPAddress.Any, 7483);

    public ServerHandler(HidHandler hidHandler)
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(_localEndpoint);

        // Stopwatch stopwatch = new Stopwatch();
        Span<byte> bytes = GC.AllocateArray<byte>(9, true);

        while (true)
        {
            if (GCSettings.LatencyMode != GCLatencyMode.NoGCRegion)
            {
                GC.TryStartNoGCRegion((1024 * 1024) * 64, (1024 * 1024) * 16, false);
            }

            if (!_hasMouse)
            {
                if (hidHandler.HidMouseHandlers.Count > 0)
                {
                    _hasMouse = true;
                }
                else
                {
                    Thread.Sleep(1);
                    continue;
                }
            }

            socket.ReceiveFrom(bytes, SocketFlags.None, ref _localEndpoint);

            if (bytes.Length > 0)
            {
                int centerX = (short) (bytes[0] | bytes[1] << 8);
                int centerY = (short) (bytes[2] | bytes[3] << 8);
                bool ignoreAim = bytes.Length > 4 && bytes[4] > 0;
                bool ignoreWait = bytes.Length > 5 && bytes[5] > 0;

                bool? left = bytes.Length > 6 ? bytes[6] > 0 ? true : null : null;
                bool? right = bytes.Length > 7 ? bytes[7] > 0 ? true : null : null;
                bool? middle = bytes.Length > 8 ? bytes[8] > 0 ? true : null : null;

                if (_hasMouse)
                {
                    if (RecoilHandler.Aiming | ignoreAim)
                    {
                        if (RecoilHandler.WaitAiming.ElapsedMilliseconds >= 550 | ignoreWait)
                        {
                            // stopwatch.Restart();
                            hidHandler.HidMouseHandlers[0].MouseLock.EnterReadLock();
                            try
                            {
                                left ??= hidHandler.HidMouseHandlers[0].Mouse.LeftButton;
                                right ??= hidHandler.HidMouseHandlers[0].Mouse.RightButton;
                                middle ??= hidHandler.HidMouseHandlers[0].Mouse.MiddleButton;

                                hidHandler.WriteGenericEvent(hidHandler.HidMouseHandlers[0].Mouse with
                                {
                                    LeftButton = (bool) left,
                                    RightButton = (bool) right,
                                    MiddleButton = (bool) middle,
                                    X = centerX,
                                    Y = centerY,
                                    Wheel = 0
                                });

                                // stopwatch.Stop();

                                // ConsoleUtils.WriteLine($"Elapsed since packet received: {stopwatch.ElapsedTicks}");
                            }
                            finally
                            {
                                hidHandler.HidMouseHandlers[0].MouseLock.ExitReadLock();
                            }
                        }
                    }
                }

                if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                {
                    GC.EndNoGCRegion();
                }
            }
        }
    }
}