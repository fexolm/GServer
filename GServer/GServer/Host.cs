using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GServer
{
    public class Datagram
    {
        public readonly byte[] Buffer;
        public readonly IPEndPoint EndPoint;
        public Datagram(byte[] buffer, IPEndPoint ipEndpoint)
        {
            Buffer = buffer;
            EndPoint = ipEndpoint;
        }
    }
    public class Host
    {
        private readonly ConcurrentQueue<Datagram> _messages;
        private readonly UdpClient _client;
        private Thread ListenThread;
        public Host(int port)
        {
            _client = new UdpClient(port);
            ListenThread = new Thread(Listen);
        }
        private void Listen()
        {
            while (true)
            {
                IPEndPoint endPoint = null;
                var buffer = _client.Receive(ref endPoint);
                var datagram = new Datagram(buffer, endPoint);
                _messages.Enqueue(datagram);
                ThreadPool.QueueUserWorkItem((o) => { ProcessDatagram(datagram); });
            }
        }

        private void ProcessDatagram(Datagram datagram)
        {
            switch ((MessageType)datagram.Buffer[0])
            {
                case MessageType.Handshake:
                    
                    break;
            }
        }
        public void StartListen()
        {
            ListenThread.Start();
        }
        public void StopListen()
        {
            ListenThread.Abort();
        }
    }
}
