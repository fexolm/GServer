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
        private readonly Thread _connectionCleaningThread;
        private readonly List<Thread> _processingThreads;
        private readonly ConnectioManager _connectionManager;
        private readonly IDictionary<Token, Connection> _connections;
        public IDictionary<MessageType, Int16> TypeCounts;              
        private bool _isListening;
        public int MessageCount;
        
        public Host(int port, int threadCount)
        {
            _client = new UdpClient(port);
            _listenThread = new Thread(Listen);
            _datagrams = new Queue<Datagram>();
            _processingThreads = new List<Thread>();
            _connections = new Dictionary<Token, Connection>();
            _isListening = false;
            _connectionManager = new ConnectioManager();
            _connectionCleaningThread = new Thread(CleanConnections);

            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(ProcessQueue);
                _processingThreads.Add(thread);
            }
        }
        private void CleanConnections()
        {

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
            var msg = Message.Deserialize(datagram.Buffer);
            switch (msg.Header.Type)
            {
                case MessageType.Handshake:
                    var connection = new Connection(datagram.EndPoint);
                    lock (_connections)
                    {
                        _connections.Add(connection.Token, connection);
                    }
                    break;
                case MessageType.Ping:
                    Connection con = null;
                    lock (_connections)
                    {
                        con = _connections[msg.Header.ConnectionToken];
                    }
                    lock (con)
                    {
                        con.UpdateActivity();
                    }
                    break;
                case MessageType.Rpc:

                    break;
                case MessageType.Authorization:
                    break;
                case MessageType.Ack:
                    AckManager.ReceiveAck(msg, this);
                    break;
            }            
        }
        public void StartListen()
        {
            _isListening = true;
            foreach (var thread in _processingThreads)
            {
                thread.Start();
            }
            _listenThread.Start();
            _connectionCleaningThread.Start();
        }
        public void StopListen()
        {
            _isListening = false;
        }
        public void Send(Message msg, IPEndPoint endPoint)
        {
            var buffer = msg.Serialize();
            _client.Send(buffer, buffer.Length, endPoint);
        }
    }
}
