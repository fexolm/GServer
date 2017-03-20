using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace GServer
{
    public delegate void ReceiveHandler(Message msg, Connection con);
    public class Host
    {
        private Token _hostToken;
        private ISocket _client;
        private readonly Thread _listenThread;
        private readonly Thread _connectionCleaningThread;
        private readonly ConnectionManager _connectionManager;
        private bool _isListening;
        private int _port;
        private IDictionary<short, IList<ReceiveHandler>> _receiveHandlers;
        private int _threadCount;
        private bool _isClinet = false;
        public Host(int port)
        {
            _listenThread = new Thread(() => Listen(port));
            _port = port;
            _isListening = false;
            _connectionManager = new ConnectionManager();
            _connectionCleaningThread = new Thread(CleanConnections);
            _receiveHandlers = new Dictionary<short, IList<ReceiveHandler>>();
            Connection.OrderedLost = (con, msg) =>
            {
                if (_isClinet)
                {
                    Send(msg);
                }
                else
                {
                    Send(msg, con);
                }
                PacketLost?.Invoke();
            };
            AddHandler((short)MessageType.Token, (m, c) =>
            {
                _hostToken = m.ConnectionToken;
                if (OnConnect != null)
                {
                    OnConnect.Invoke();
                }
            });
            AddHandler((short)MessageType.Handshake, (m, c) =>
            {
                SendToken(c);
            });
            AddHandler((short)MessageType.Ack, (m, c) =>
            {
                c.ProcessAck(m);
            });
        }
        private void SendToken(Connection con)
        {
            Message msg = new Message((short)MessageType.Token, Mode.None);
            msg.ConnectionToken = con.Token;
            Send(msg, con);
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
            _client.Bind(new IPEndPoint(IPAddress.Any, port));
            while (_isListening)
            {
                if (_client.Available > 0)
                {
                    IPEndPoint endPoint = null;
                    var buffer = _client.Receive(ref endPoint);

                    if (buffer.Length == 0)
                        return;

                    var msg = Message.Deserialize(buffer);
                    Connection connection;
                    if (_connectionManager.TryGetConnection(out connection, msg, endPoint))
                    {
                        ProcessDatagram(msg, connection);
                    }
                }
            }
        }
        private void ProcessDatagram(Message msg, Connection connection)
        {
            if (msg.Header.Reliable)
            {
                var ack = connection.GenerateAck(msg);
                var bitField = new DataStorage(ack.Body).ReadInt32();
                if (!_isClinet)
                {
                    Send(ack, connection);
                }
                else
                {
                    Send(ack);
                }
                Console.WriteLine(bitField);
            }

            if (msg.Header.Sequenced)
            {
                if (connection.IsMessageInItsOrder(msg.Header.Type, msg.Header.MessageId))
                {
                    ProcessHandler(msg, connection);
                }
            }
            else if (msg.Header.Ordered)
            {
                var toInvoke = connection.MessagesToInvoke(msg);
                if (toInvoke == null)
                {
                    return;
                }
                if (_threadCount > 0)
                {
                    ThreadPool.QueueUserWorkItem((o) => ProcessHandlerList(toInvoke, connection));
                }
                else
                {
                    ProcessHandler(msg, connection);
                }
            }
            else if (_threadCount > 0)
            {
                ThreadPool.QueueUserWorkItem((o) => ProcessHandler(msg, connection));
            }
            else
            {
                ProcessHandler(msg, connection);
            }
        }
        private void ProcessHandler(Message msg, Connection connection)
        {
            IList<ReceiveHandler> handlers = null;
            lock (_receiveHandlers)
            {
                if (_receiveHandlers.ContainsKey((short)msg.Header.Type))
                {
                    handlers = _receiveHandlers[(short)msg.Header.Type];
                }
            }
            if (handlers != null)
            {
                foreach (var h in handlers)
                {
                    try
                    {
                        h.Invoke(msg, connection);

                    }
                    catch (Exception ex)
                    {
                        WriteError(ex.Message);
                    }
                }
                connection.UpdateActivity();
            }
        }
        private void ProcessHandlerList(List<Message> messages, Connection connection)
        {
            IList<ReceiveHandler> handlers = null;
            if (messages.Count == 0)
            {
                return;
            }
            var msg = messages[0];
            lock (_receiveHandlers)
            {
                if (_receiveHandlers.ContainsKey((short)msg.Header.Type))
                {
                    handlers = _receiveHandlers[(short)msg.Header.Type];
                }
            }
            if (handlers != null)
            {
                connection = _connectionManager[msg.Header.ConnectionToken];
                foreach (var h in handlers)
                {
                    foreach (var m in messages)
                    {
                        try
                        {
                            h.Invoke(m, connection);
                        }
                        catch (Exception ex)
                        {
                            WriteError(ex.Message);
                        }
                    }
                }
                connection.UpdateActivity();
            }
        }
        public void StartListen(int threadCount, ISocket host)
        {
            if (threadCount > 0)
            {
                ThreadPool.SetMinThreads(threadCount, threadCount);
            }
            _threadCount = threadCount;
            _isListening = true;
            _client = host;
            _listenThread.Start();
            _connectionCleaningThread.Start();
        }
        public void StartListen(int threadCount)
        {
            StartListen(threadCount, new HostImpl());
        }
        public void StopListen()
        {
            _isListening = false;
            _client.Close();
        }
        public void Send(Message msg, Connection con)
        {
            try
            {
                msg.ConnectionToken = con.Token;
                if (msg.Header.Type != (short)MessageType.Ack)
                    msg.MessageId = con.GetMessageId(msg);
                var buffer = msg.Serialize();
                _client.Send(buffer, con.EndPoint);
                if (msg.Header.Reliable)
                {
                    con.StoreReliable(msg);
                }
            }
            catch (Exception ex)
            {
                ErrLog.Invoke(ex.Message);
            }
        }
        public void Send(Message msg)
        {
            try
            {
                Connection connection;
                lock (_connectionManager)
                {
                    connection = _connectionManager[_hostToken];
                }
                if (msg.Header.Type != (short)MessageType.Ack)
                    msg.MessageId = connection.GetMessageId(msg);
                msg.ConnectionToken = _hostToken;
                var buffer = msg.Serialize();
                _client.Send(buffer);
                if (msg.Header.Reliable)
                {
                    connection.StoreReliable(msg);
                }
            }
            catch (Exception ex)
            {
                ErrLog.Invoke(ex.Message);
            }
        }
        public void AddHandler(short type, ReceiveHandler handler)
        {
            lock (_receiveHandlers)
            {
                if (_receiveHandlers.ContainsKey(type))
                {
                    _receiveHandlers[type].Add(handler);
                }
                else
                {
                    IList<ReceiveHandler> list = new List<ReceiveHandler>();
                    list.Add(handler);
                    _receiveHandlers.Add(type, list);
                }
            }
        }
        public bool BeginConnect(IPEndPoint ep)
        {
            try
            {
                _client.Connect(ep);
                _isClinet = true;
            }
            catch
            {
                return false;
            }
            var buffer = Message.Handshake.Serialize();
            _client.Send(buffer);
            return true;
        }
        internal void WriteError(string error)
        {
            if (ErrLog != null)
            {
                ErrLog.Invoke(error);
            }
        }
        internal void WriteDebug(string error)
        {
            if (DebugLog != null)
            {
                DebugLog.Invoke(error);
            }
        }
        public Action<string> ErrLog;
        public Action<string> DebugLog;
        public Action OnConnect;
        public event Action PacketLost;
        ~Host()
        {
            StopListen();
        }
    }
}
