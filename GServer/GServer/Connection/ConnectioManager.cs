using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GServer
{
    public class ConnectionManager
    {
        private SortedDictionary<Token, Connection> _connections;
        public ConnectionManager()
        {
            _connections = new SortedDictionary<Token, Connection>();
        }
        public void Add(Token token, Connection con)
        {
            _connections.Add(token, con);
        }
        public void Remove(Token token)
        {
            _connections.Remove(token);
        }
        public void RemoveNotActive()
        {
            List<Token> toRemove = new List<Token>();
            foreach (var element in _connections)
            {
                if ((DateTime.Now - element.Value.LastActivity).TotalSeconds > 30)
                {
                    toRemove.Add(element.Key);
                    element.Value.Disconnect();
                }
            }
            foreach (var key in toRemove)
            {
                _connections.Remove(key);
            }
        }

        public Connection this[Token key]
        {
            get
            {
                return _connections[key];
            }
            set
            {
                _connections[key] = value;
            }
        }
        public bool TryGetConnection(out Connection con, Message msg, IPEndPoint endPoint)
        {
            lock (_connections)
            {
                if (msg.Header.ConnectionToken != null &&
                    _connections.ContainsKey(msg.Header.ConnectionToken))
                {
                    con = _connections[msg.Header.ConnectionToken];
                    return true;
                }
            }
            if (msg.Header.Type == MessageType.Handshake)
            {
                con = new Connection(endPoint);
                lock (_connections)
                {
                    _connections.Add(con.Token, con);
                }
                if (HandshakeRecieved != null)
                {
                    HandshakeRecieved.Invoke(con);

                }
                return true;
            }
            con = null;
            return false;
        }
        public event Action<Connection> HandshakeRecieved;
    }
}
