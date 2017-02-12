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
        private UdpClient _client;
        private readonly Thread _listenThread;
        private readonly Thread _connectionCleaningThread;
        private readonly List<Thread> _processingThreads;
        private readonly ConnectionManager _connectionManager;
        private bool _isListening;
        private IDictionary<short, Action<Message, EndPoint>> _ReceiveHandlers;
        public Host(int port)
        {
            _listenThread = new Thread(() => Listen(port));
            _datagrams = new Queue<Datagram>();
            _processingThreads = new List<Thread>();
            _isListening = false;
            _connectionManager = new ConnectionManager();
            _connectionCleaningThread = new Thread(CleanConnections);
        }
        private void CleanConnections()
        {
            lock (_connectionManager)
            {
                _connectionManager.RemoveNotActive();
            }
        }
        private void Listen(int port)
        {
            _client = new UdpClient();
            _client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            while (_isListening && _client.Available > 0)
            {
                IPEndPoint endPoint = null;
                var buffer = _client.Receive(ref endPoint);
                var datagram = new Datagram(buffer, endPoint);
                if (_processingThreads.Count > 0)
                {
                    lock (_datagrams)
                    {
                        _datagrams.Enqueue(datagram);
                    }
                }
                else
                {
                    ProcessDatagram(datagram);
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
            if (datagram.Buffer.Length == 0)
                return;
            var msg = Message.Deserialize(datagram.Buffer);
            bool contains = false;
            lock (_ReceiveHandlers)
            {
                if (!_ReceiveHandlers.ContainsKey((short)msg.Header.Type))
                {
                    contains = true;
                }
                else
                {
                    _ReceiveHandlers[(short)msg.Header.Type].Invoke(msg, datagram.EndPoint);
                }
            }
            if (contains)
            {
                throw new Exception("no handler for this msgType");
            }
        }
        public void StartListen(int threadCount)
        {
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(ProcessQueue);
                _processingThreads.Add(thread);
            }
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
            _processingThreads.Clear();
            _client.Close();
        }
        public void Send(Message msg, IPEndPoint endPoint)
        {
            var buffer = msg.Serialize();
            _client.Send(buffer, buffer.Length, endPoint);
        }
        public void AddHandler(short type, Action<Message, EndPoint> handler)
        {
            bool contains = false;
            lock (_ReceiveHandlers)
            {
                if (_ReceiveHandlers.ContainsKey(type))
                {
                    contains = true;
                }
                else
                {
                    _ReceiveHandlers.Add(type, handler);
                }
            }
            if (contains)
            {
                throw new Exception("Hander for this msgtype is already registered");
            }
        }
    }
}
