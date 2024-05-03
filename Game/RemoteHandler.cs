using System.Net;
using System.Net.Sockets;
using HID_API;

namespace Helious.Game;

public class RemoteHandler
{
    private readonly Mouse _localState = new();
    
    private static readonly Socket Socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private static readonly IPAddress Broadcast = IPAddress.Parse("192.168.0.159");
    private static readonly IPEndPoint EndPoint = new(Broadcast, 7484);
    
    public RemoteHandler(HidHandler hidHandler)
    {
        Socket.Connect(EndPoint);
        
        while (true)
        {
            var mouseState = hidHandler.HidMouseHandlers[0].GetMouseState();

            if (_localState != mouseState)
            {
                var data = PreparePacket(mouseState);
                Socket.Send(data);
            }
            
            _localState = mouseState;
        }
    }
    
    private static byte[] PreparePacket(Mouse state)
    {
        return new[]
        {
            (byte) (state.LeftButton ? 1 : 0),
            (byte) (state.RightButton ? 1 : 0),
            (byte) (state.MiddleButton ? 1 : 0),
            (byte) (state.FourButton ? 1 : 0),
            (byte) (state.FiveButton ? 1 : 0),
            (byte) (state.X >> 0), (byte) (state.X >> 8),
            (byte) (state.Y >> 0), (byte) (state.Y >> 8)
        };
    }
}