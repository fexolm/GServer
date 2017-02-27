using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GServer
{
    public delegate void ReceiveHandler(Message msg, Connection con);
    public class Host
    {
        private Token _hostToken;
        private UdpClient _client;
        private readonly Thread _listenThread;
        private readonly Thread _connectionCleaningThread;
        private readonly ConnectionManager _connectionManager;
        private bool _isListening;
        private IDictionary<short, IList<ReceiveHandler>> _receiveHandlers;
        private int _threadCount;
        public Host(int port)
        {
            _listenThread = new Thread(() => Listen(port));
            _isListening = false;
            _connectionManager = new ConnectionManager();
            _connectionCleaningThread = new Thread(CleanConnections);
            _receiveHandlers = new Dictionary<short, IList<ReceiveHandler>>();
            _connectionManager.HandshakeRecieved += SendToken;
        }
        private void SendToken(Connection con)
        {
            Message msg = new Message(MessageType.Token, Mode.None, null);
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
            _client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
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
                Send(connection.GenerateAck(msg), connection);
            }

            if (msg.Header.Sequenced)
            {
                if (connection.IsMessageInItsOrder((short)msg.Header.Type, msg.Header.MessageId))
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
                connection = _connectionManager[msg.Header.ConnectionToken];
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
        public void StartListen(int threadCount)
        {
            if (threadCount > 0)
            {
                ThreadPool.SetMinThreads(threadCount, threadCount);
            }
            _threadCount = threadCount;
            _isListening = true;
            _client = new UdpClient();
            _client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenThread.Start();
            _connectionCleaningThread.Start();
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
                msg.MessageId = con.GetMessageId(msg);
                var buffer = msg.Serialize();
                _client.Send(buffer, buffer.Length, con.EndPoint);
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
                msg.MessageId = connection.GetMessageId(msg);
                msg.ConnectionToken = _hostToken;
                var buffer = msg.Serialize();
                DebugLog.Invoke(msg.MessageId.ToString());
                _client.Send(buffer, buffer.Length);
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
        public bool Connect(IPEndPoint ep)
        {
            try
            {
                _client.Connect(ep);
            }
            catch
            {
                return false;
            }
            var buffer = Message.Handshake.Serialize();
            _client.Send(buffer, buffer.Length);
            IPEndPoint remoteEp = null;
            while (true)
            {
                byte[] recieved = _client.Receive(ref remoteEp);
                if (remoteEp.Address.ToString() == ep.Address.ToString() && remoteEp.Port == ep.Port)
                {
                    var msg = Message.Deserialize(recieved);
                    _hostToken = msg.ConnectionToken;
                    Connection con = new Connection(ep, _hostToken);
                    lock (_connectionManager)
                    {
                        _connectionManager.Add(con.Token, con);
                    }
                    break;
                }
            }
            return true;
        }
        public void WriteError(string error)
        {
            if (ErrLog != null)
            {
                ErrLog.Invoke(error);
            }
        }
        public void WriteDebug(string error)
        {
            if (DebugLog != null)
            {
                DebugLog.Invoke(error);
            }
        }
        public Action<string> ErrLog;
        public Action<string> DebugLog;
    }
}
