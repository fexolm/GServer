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
        }

        public bool MulticastLoopback
        {
            get => _client.MulticastLoopback;
            set => _client.MulticastLoopback = value;
        }
        public bool DontFragment
        {
            get => _client.DontFragment;
            set => _client.DontFragment = value;
        }
        public short Ttl
        {
            get => _client.Ttl;
            set => _client.Ttl = value;
        }

        public int Available => _client.Available;

        public Socket Client
        {
            get => _client.Client;
            set => _client.Client = value;
        }
        public bool EnableBroadcast
        {
            get => _client.EnableBroadcast;
            set => _client.EnableBroadcast = value;
        }
        public bool ExclusiveAddressUse
        {
            get => _client.ExclusiveAddressUse;
            set => _client.ExclusiveAddressUse = value;
        }

        public void Close()
        {
            _client.Close();
        }

        public void Connect(string hostname, int port)
        {
            _client.Connect(hostname, port);
        }

        public void Connect(IPAddress addr, int port)
        {
            _client.Connect(addr, port);
        }

        public void Connect(IPEndPoint endPoint)
        {
            _client.Connect(endPoint);
        }

        public byte[] Receive(ref IPEndPoint remoteEP)
        {
            return _client.Receive(ref remoteEP);
        }

        public int Send(byte[] dgram, int bytes, string hostname, int port)
        {
            return Send(dgram, bytes, hostname, port);
        }

        public int Send(byte[] dgram, int bytes, IPEndPoint endPoint)
        {
            return _client.Send(dgram, bytes, endPoint);
        }

        public int Send(byte[] dgram, int bytes)
        {
            return _client.Send(dgram, bytes);
        }
    }
}
