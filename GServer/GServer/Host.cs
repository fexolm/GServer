using System;
using System.Collections.Generic;
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
        private readonly Queue<Datagram> _datagrams;
        private readonly UdpClient _client;
        private readonly Thread _listenThread;
        private readonly List<Thread> _processingThreads;
        private readonly IDictionary<Token, Connection> _connections;
        private bool _isListening;
        public Host(int port, int threadCount)
        {
            _client = new UdpClient(port);
            _listenThread = new Thread(Listen);
            _datagrams = new Queue<Datagram>();
            _processingThreads = new List<Thread>();
            _connections = new Dictionary<Token, Connection>();
            _isListening = false;
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(ProcessQueue);
                _processingThreads.Add(thread);
                thread.Start();
            }
        }
        private void Listen()
        {
            while (_isListening && _client.Available > 0)
            {
                IPEndPoint endPoint = null;
                var buffer = _client.Receive(ref endPoint);
                var datagram = new Datagram(buffer, endPoint);
                lock (_datagrams)
                {
                    _datagrams.Enqueue(datagram);
                }
            }
        }
        private void ProcessQueue()
        {
            while (_isListening)
            {
                Datagram prcessDgram = null;
                lock (_datagrams)
                {
                    if (_datagrams.Count != 0)
                    {
                        prcessDgram = _datagrams.Dequeue();
                    }
                }
                if (prcessDgram == null)
                    Thread.Sleep(1000);
                else
                    ProcessDatagram(prcessDgram);
            }
        }
        private void ProcessDatagram(Datagram datagram)
        {
            _client.Send(datagram.Buffer, datagram.Buffer.Length, datagram.EndPoint);
            Console.WriteLine(datagram.EndPoint.Address + ":" + datagram.EndPoint.Port);
            switch ((MessageType)datagram.Buffer[0])
            {
                case MessageType.Handshake:
                    lock (_connections)
                    {
                        var connection = new Connection(datagram.EndPoint);
                        _connections.Add(connection.Token, connection);
                    }
                    break;
                case MessageType.Ping:
                    lock (_connections)
                    {

                    }
                    break;
            }
        }
        public void StartListen()
        {
            _isListening = true;
            _listenThread.Start();
        }
        public void StopListen()
        {
            _isListening = false;
        }
    }
}
