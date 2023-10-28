using System.Net;
using System.Net.Sockets;
using System.Text;
using Helious.Utils;
using HID_API;

namespace Helious.Game;

public class ServerHandler
{
    public ServerHandler(HidHandler hidHandler)
    {
        var localEndpoint = new IPEndPoint(IPAddress.Any, 7483);
        var udpServer = new UdpClient(localEndpoint);
        
        while (true)
        {
            byte[] bytes = udpServer.Receive(ref localEndpoint);
        
            var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            var splitData = data.Split(",");
                    
            var centerX = float.Parse(splitData[0]);
            var centerY = float.Parse(splitData[1]);
        
            if (hidHandler.HidMouseHandlers.Count > 0)
            {
                if (RecoilHandler.Aiming)
                {
                    if (RecoilHandler.WaitAiming.ElapsedMilliseconds >= 550)
                    {
                        ConsoleUtils.WriteLine($"Positional data (X: {centerX}, Y: {centerY}, {RecoilHandler.WaitAiming.ElapsedMilliseconds})");
                        
                        hidHandler.HidMouseHandlers[0].mouseLock.EnterReadLock();
                        try
                        {
                            hidHandler.WriteMouseReport(hidHandler.HidMouseHandlers[0].Mouse with
                            {
                                X = centerX,
                                Y = centerY,
                                Wheel = 0
                            });
                        }
                        finally
                        {
                            hidHandler.HidMouseHandlers[0].mouseLock.ExitReadLock();
                        }
                    }
                }
            }
        }
    }
}