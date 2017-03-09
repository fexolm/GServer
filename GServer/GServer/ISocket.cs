using System.Net;
using System.Net.Sockets;

namespace GServer
{
    public interface ISocket
    {
        bool MulticastLoopback { get; set; }
        bool DontFragment { get; set; }
        short Ttl { get; set; }
        int Available { get; }
        Socket Client { get; set; }
        bool EnableBroadcast { get; set; }
        bool ExclusiveAddressUse { get; set; }
        void Close();
        void Connect(string hostname, int port);
        void Connect(IPAddress addr, int port);
        void Connect(IPEndPoint endPoint);
        byte[] Receive(ref IPEndPoint remoteEP);
        int Send(byte[] dgram, int bytes, string hostname, int port);
        int Send(byte[] dgram, int bytes, IPEndPoint endPoint);
        int Send(byte[] dgram, int bytes);
    }
}