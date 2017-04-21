using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace GServer
{
    public delegate void ReceiveHandler(Message msg, Connection con);
    public class Host : IDisposable
    {
        private Token _hostToken;
        private ISocket _client;
        private readonly Thread _listenThread;
        private readonly ConnectionManager _connectionManager;
        private bool _isListening;
        private int _port;
        private IDictionary<short, IList<ReceiveHandler>> _receiveHandlers;
        private int _threadCount;
        private uint _connectionCleaningTick = 0;
        public uint ConnectionCleaningInterval { get; set; }
        public Host(int port)
        {
            _listenThread = new Thread(() => Listen(port));
            _port = port;
            _isListening = false;
            _connectionManager = new ConnectionManager();
            _receiveHandlers = new Dictionary<short, IList<ReceiveHandler>>();
            Rnd = new Random();
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
            ServerTimer.OnTick += ServerTick;
        }
        private void ServerTick()
        {
            //_connectionCleaningTick++;
            //if (_connectionCleaningTick > ConnectionCleaningInterval)
            //{
            //    CleanConnections();
            //    _connectionCleaningTick = 0;
            //}
            _connectionManager.InvokeForAllConnections(c =>
            {
                byte[] buffer = c.GetBytesToSend();
                if (buffer.Length > 0)
                {
                    _client.Send(buffer, c.EndPoint);
                }
            });
            OnTick?.Invoke();
        }
        private void SendToken(Connection con)
        {
            Message msg = new Message((short)MessageType.Token, Mode.None);
            msg.ConnectionToken = con.Token;
            Send(msg, con);
        }
        public void CleanConnections()
        {
            _connectionManager.RemoveNotActive();
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

                    Console.WriteLine(endPoint.ToString());
                    if (buffer.Length == 0)
                        continue;
                    var ds = new DataStorage(buffer);
                    while (!ds.Empty)
                    {
                        int len = ds.ReadInt32();
                        var msg = Message.Deserialize(ds.ReadBytes(len));
                        Connection connection;
                        if (_connectionManager.TryGetConnection(out connection, msg, endPoint))
                        {
                            ProcessDatagram(msg, connection);
                        }
                    }
                }
            }
        }
        private void ProcessDatagram(Message msg, Connection connection)
        {
            if (msg.Header.Reliable)
            {
                connection.ReceiveReliable(msg);
                if (!msg.Header.Sequenced && !msg.Header.Ordered)
                {
                    if (connection.HasAlreadyArrived(msg))
                    {
                        return;
                    }
                }
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
                    ProcessHandlerList(toInvoke, connection);
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
            connection.InvokeIfBinded(msg);
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
                    h.Invoke(msg, connection);
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
        }
        public void StartListen(int threadCount)
        {
            StartListen(threadCount, new HostImpl());
        }
        public void StopListen()
        {
            _isListening = false;
            ServerTimer.OnTick -= ServerTick;
            _client.Close();
        }
        public void Send(Message msg, Connection con)
        {
            msg.ConnectionToken = con.Token;
            if (msg.Header.Type != (short)MessageType.Ack)
                msg.MessageId = con.GetMessageId(msg);
            con.MarkToSend(msg);
            if (msg.Header.Reliable)
            {
                con.StoreReliable(msg);
            }
        }
        public void Send(Message msg)
        {
            Connection connection;

            connection = _connectionManager[_hostToken];
            if (msg.Reliable)
                msg.MessageId = connection.GetMessageId(msg);
            msg.ConnectionToken = _hostToken;
            connection.MarkToSend(msg);
            if (msg.Header.Reliable)
            {
                connection.StoreReliable(msg);
            }
        }
        internal void RowSend(Message msg, Connection con)
        {
            var buffer = msg.Serialize();
            con.MarkToSend(msg);
            if (msg.Header.Reliable)
            {
                con.StoreReliable(msg);
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
            }
            catch
            {
                return false;
            }
            var buffer = Message.Handshake;
            Packet p = new Packet(buffer);
            _client.Send(p.Serialize());
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
        public void AddModule(IPlugin module)
        {
            module.Bind(this);
        }
        public readonly Random Rnd;
        public void Dispose()
        {
            StopListen();
        }
        public Action<string> ErrLog;
        public Action<string> DebugLog;
        public Action OnConnect;
        public void Tick()
        {
            ServerTimer.Tick();
        }
        public IEnumerable<Connection> GetConnections()
        {
            List<Connection> res = new List<Connection>();
            _connectionManager.InvokeForAllConnections(c => res.Add(c));
            return res;
        }
        ~Host()
        {
            StopListen();
        }
        public event Action OnTick;
    }
}
