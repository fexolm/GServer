using System.Net;
using System.Net.Sockets;

namespace GServer
{
    class HostImpl : ISocket
    {
        private UdpClient _client;
        public HostImpl()
        {
            _client = new UdpClient();
            _client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public int Available => _client.Available;

        public void Bind(IPEndPoint localEP)
        {
            _client.Client.Bind(localEP);
        }

        public void Close()
        {
            _client.Close();
        }

        public void Connect(IPEndPoint endPoint)
        {
            _client.Connect(endPoint);
        }

        public void Dispose()
        {
            _client.Close();
        }

        public byte[] Receive(ref IPEndPoint remoteEP)
        {
            return _client.Receive(ref remoteEP);
        }

        public int Send(byte[] dgram)
        {
            return _client.Send(dgram, dgram.Length);
        }
        public int Send(byte[] dgram, IPEndPoint endPoint)
        {
            return _client.Send(dgram, dgram.Length, endPoint);
        }
    }
}
