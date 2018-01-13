using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using GServer.Containers;
using System.Linq;
using GServer.Connection;
using GServer.Messages;

// ReSharper disable UnusedMember.Global
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable UseNullPropagation

namespace GServer
{
    public class AllowedTokens
    {
        private readonly Token[] _allowedTokens;
        private readonly bool _allowAll;

        private AllowedTokens() {
            _allowAll = true;
        }

        public AllowedTokens(params Token[] tokens) {
            _allowAll = false;
            _allowedTokens = tokens;
        }

        public static readonly AllowedTokens Any = new AllowedTokens();

        internal bool IsAccepted(Token token) {
            return _allowAll || _allowedTokens.Contains(token);
        }
    }

    public delegate void ReceiveHandler(Message msg, Connection.Connection con);

    public class Host : IDisposable
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public bool EnableHandshake { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public AllowedTokens AllowedTokens { get; set; }

        private Token _hostToken;
        private ISocket _client;
        private readonly Thread _listenThread;
        private readonly ConnectionManager _connectionManager;
        private bool _isListening;
        private readonly IDictionary<short, IList<ReceiveHandler>> _receiveHandlers;
        private int _connectionCleaningTick = 0;

        /// <summary>
        /// Interval in server ticks of disconnecting inactive connections 
        /// </summary>
        public uint ConnectionCleaningInterval { get; set; }

        public Host(int port) {
            EnableHandshake = true;
            AllowedTokens = AllowedTokens.Any;
            //ValidateMessageTypes();
            _listenThread = new Thread(() => Listen(port));
            _isListening = false;
            _connectionManager = new ConnectionManager();
            _receiveHandlers = new Dictionary<short, IList<ReceiveHandler>>();
            AddHandler((short) MessageType.Token, (m, c) => {
                _hostToken = m.ConnectionToken;
                if (OnConnect != null) {
                    OnConnect.Invoke();
                }
            });
            AddHandler((short) MessageType.Handshake, (m, c) => {
                if (!EnableHandshake) return;
                if (ConnectionCreated != null) {
                    ConnectionCreated.Invoke(c);
                }
                SendToken(c);
            });
            AddHandler((short) MessageType.Ack, (m, c) => { c.ProcessAck(m); });
            ServerTimer.OnTick += ServerTick;
        }

        /// <summary>
        /// Disconnect connection
        /// </summary>
        /// <param name="con">Connection to disconnect</param>
        public void ForceDisconnect(Connection.Connection con) {
            _connectionManager.Remove(con.Token);
        }

        private void ServerTick() {
            _connectionManager.InvokeForAllConnections(c => {
                var buffer = c.GetBytesToSend();
                if (buffer.Length > 0) {
                    _client.Send(buffer, c.EndPoint);
                }
            });
            if (OnTick != null) OnTick.Invoke();
        }

        private void SendToken(Connection.Connection con) {
            var msg = new Message((short) MessageType.Token, Mode.None) {ConnectionToken = con.Token};
            Send(msg, con);
        }

        /// <summary>
        /// Cleaning inactive connections
        /// </summary>
        public void CleanConnections() {
            _connectionManager.RemoveNotActive();
        }

        private void Listen(int port) {
            _client.Bind(new IPEndPoint(IPAddress.Any, port));
            while (_isListening) {
                if (_client.Available <= 0) continue;
                IPEndPoint endPoint = null;
                var buffer = _client.Receive(ref endPoint);
                if (buffer == null) {
                    continue;
                }
                if (buffer.Length == 0)
                    continue;
                var ds = DataStorage.CreateForRead(buffer);
                while (!ds.Empty) {
                    var len = ds.ReadInt32();
                    var msg = Message.Deserialize(ds.ReadBytes(len));
                    Connection.Connection connection;
                    if (!_connectionManager.TryGetConnection(out connection, msg, endPoint)) continue;
                    if (AllowedTokens.IsAccepted(connection.Token)) {
                        ProcessDatagram(msg, connection);
                    }
                    else {
                        ForceDisconnect(connection);
                    }
                }
            }
        }

        private static void InvokeHandler(ReceiveHandler handler, Message msg, Connection.Connection connection) {
            var async = handler.GetType().GetCustomAttributes(typeof(AsyncOperationAttribute), false).Length > 0;
            if (async) {
                ThreadPool.QueueUserWorkItem((o) => handler.Invoke(msg, connection));
            }
            else {
                handler.Invoke(msg, connection);
            }
        }

        private void ProcessDatagram(Message msg, Connection.Connection connection) {
            if (msg.Header.Reliable) {
                connection.ReceiveReliable(msg);
                if (!msg.Header.Sequenced && !msg.Header.Ordered) {
                    if (connection.HasAlreadyArrived(msg)) {
                        return;
                    }
                }
            }
            if (msg.Header.Sequenced) {
                if (connection.IsMessageInItsOrder(msg.Header.Type, msg.Header.MessageId)) {
                    ProcessHandler(msg, connection);
                }
            }
            else if (msg.Header.Ordered) {
                var toInvoke = connection.MessagesToInvoke(msg);
                if (toInvoke == null) {
                    return;
                }
                ProcessHandlerList(toInvoke, connection);
            }
            else {
                ProcessHandler(msg, connection);
            }
        }

        private void ProcessHandler(Message msg, Connection.Connection connection) {
            connection.InvokeIfBinded(msg);
            IList<ReceiveHandler> handlers = null;
            lock (_receiveHandlers) {
                if (_receiveHandlers.ContainsKey(msg.Header.Type)) {
                    handlers = _receiveHandlers[msg.Header.Type];
                }
            }
            if (handlers == null) return;
            foreach (var h in handlers) {
                InvokeHandler(h, msg, connection);
            }
            connection.UpdateActivity();
        }

        private void ProcessHandlerList(IList<Message> messages, Connection.Connection connection) {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            IList<ReceiveHandler> handlers = null;
            if (messages.Count == 0) {
                return;
            }
            var msg = messages[0];
            lock (_receiveHandlers) {
                if (_receiveHandlers.ContainsKey(msg.Header.Type)) {
                    handlers = _receiveHandlers[msg.Header.Type];
                }
            }
            if (handlers == null) return;
            connection = _connectionManager[msg.Header.ConnectionToken];
            foreach (var h in handlers) {
                foreach (var m in messages) {
                    try {
                        InvokeHandler(h, m, connection);
                    }
                    catch (Exception ex) {
                        OnException.Invoke(ex);
                    }
                }
            }
            connection.UpdateActivity();
        }

        /// <summary>
        /// Begin listening (use this if u need to substitude socket implementation)
        /// </summary>
        /// <param name="host">Socket implementation</param>
        public void StartListen(ISocket host) {
            ThreadPool.SetMinThreads(Environment.ProcessorCount, Environment.ProcessorCount * 4);
            _isListening = true;
            _client = host;
            _listenThread.Start();
        }

        /// <summary>
        ///  Begin listening
        /// </summary>
        public void StartListen() {
            StartListen(new HostImpl());
        }

        /// <summary>
        /// Stop all server threads
        /// </summary>
        public void StopListen() {
            _isListening = false;
            ServerTimer.OnTick -= ServerTick;
            if (_client != null)
                _client.Close();
        }

        /// <summary>
        /// Sending message to specific connection
        /// </summary>
        /// <param name="msg">Message to send</param>
        /// <param name="con">Destination connection</param>
        public void Send(Message msg, Connection.Connection con) {
            msg.ConnectionToken = con.Token;
            if (msg.Header.Type != (short) MessageType.Ack)
                msg.MessageId = con.GetMessageId(msg);
            con.MarkToSend(msg);
            if (msg.Header.Reliable) {
                con.StoreReliable(msg);
            }
        }

        /// <summary>
        /// Send message to connected connection
        /// </summary>
        /// <param name="msg">Message to send</param>
        public void Send(Message msg) {
            var connection = _connectionManager[_hostToken];
            if (msg.Reliable)
                msg.MessageId = connection.GetMessageId(msg);
            msg.ConnectionToken = _hostToken;
            connection.MarkToSend(msg);
            if (msg.Header.Reliable) {
                connection.StoreReliable(msg);
            }
        }

        internal void RowSend(Message msg, Connection.Connection con) {
            con.MarkToSend(msg);
            if (msg.Header.Reliable) {
                con.StoreReliable(msg);
            }
        }

        /// <summary>
        /// Add handler to message specific message type
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="handler">handler to process messages with selected type</param>
        public void AddHandler(short type, ReceiveHandler handler) {
            lock (_receiveHandlers) {
                if (_receiveHandlers.ContainsKey(type)) {
                    _receiveHandlers[type].Add(handler);
                }
                else {
                    IList<ReceiveHandler> list = new List<ReceiveHandler>();
                    list.Add(handler);
                    _receiveHandlers.Add(type, list);
                }
            }
        }

        /// <summary>
        /// Send to server request to connect
        /// </summary>
        /// <param name="ep">server endpoint</param>
        /// <returns></returns>
        public bool BeginConnect(IPEndPoint ep) {
            try {
                _client.Connect(ep);
            }
            catch {
                return false;
            }
            var buffer = Message.Handshake;
            var p = new Packet(buffer);
            _client.Send(p.Serialize());
            return true;
        }

        public bool BeginConnect(IPEndPoint ep, Token token) {
            _hostToken = token;
            _connectionManager.Add(token, new Connection.Connection(ep, token));
            OnConnect.Invoke();
            return true;
        }

        internal void WriteDebug(string error) {
            if (DebugLog != null) {
                DebugLog.Invoke(error);
            }
        }

        /// <summary>
        /// Add plugin
        /// </summary>
        /// <param name="module">Plugin implementation</param>
        public void AddModule(IPlugin module) {
            module.Bind(this);
        }

        /// <summary>
        /// Server random
        /// </summary>
        public static readonly Random Rnd = new Random();

        public void Dispose() {
            StopListen();
        }

        /// <summary>
        /// Host errors
        /// </summary>
        public Action<Exception> OnException;

        /// <summary>
        /// Host debug messages
        /// </summary>
        public Action<string> DebugLog;

        /// <summary>
        /// Handle server response before BeginConnect
        /// </summary>
        public Action OnConnect;

        /// <summary>
        /// Perform host tick
        /// </summary>
        public void Tick() {
            ServerTimer.Tick();
        }

        /// <summary>
        /// Shows all connected to host connections 
        /// </summary>
        /// <returns>Connected to host connections</returns>
        public IEnumerable<Connection.Connection> GetConnections() {
            var res = new List<Connection.Connection>();
            _connectionManager.InvokeForAllConnections(c => res.Add(c));
            return res;
        }

        ~Host() {
            StopListen();
        }

        public event Action OnTick;
        public event Action<Connection.Connection> ConnectionCreated;

        public bool Cross(Pair<int, int> a, Pair<int, int> b) {
            if (a.Val1 < b.Val1) {
                return a.Val2 > b.Val1;
            }
            return b.Val2 > a.Val1;
        }
    }
}