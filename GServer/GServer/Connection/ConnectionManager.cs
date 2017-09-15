using System;
using System.Collections.Generic;
using System.Net;
using GServer.Messages;

namespace GServer.Connection
{
    internal class ConnectionManager
    {
        private readonly IDictionary<Token, Connection> _connections;

        public ConnectionManager() {
            _connections = new Dictionary<Token, Connection>();
        }

        public void Add(Token token, Connection con) {
            lock (_connections) {
                _connections.Add(token, con);
            }
        }

        public void Remove(Token token) {
            lock (_connections) {
                if (!_connections.ContainsKey(token)) return;
                _connections[token].Disconnect();
                _connections.Remove(token);
            }
        }

        public void RemoveNotActive() {
            lock (_connections) {
                var toRemove = new List<Token>();
                foreach (var element in _connections) {
                    if (!((DateTime.Now - element.Value.LastActivity).TotalSeconds > 30)) continue;
                    toRemove.Add(element.Key);
                    element.Value.Disconnect();
                }
                foreach (var key in toRemove) {
                    _connections.Remove(key);
                }
            }
        }

        public Connection this[Token key] {
            get {
                lock (_connections) {
                    return _connections[key];
                }
            }
            set {
                lock (_connections) {
                    _connections[key] = value;
                }
            }
        }

        public bool TryGetConnection(out Connection con, Message msg, IPEndPoint endPoint) {
            lock (_connections) {
                if (msg.Header.ConnectionToken != null &&
                    _connections.ContainsKey(msg.Header.ConnectionToken)) {
                    con = _connections[msg.Header.ConnectionToken];
                    if (con.EndPoint == null) {
                        con.EndPoint = endPoint;
                    }
                    return true;
                }
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (msg.Header.Type == (short) MessageType.Handshake) {
                    con = new Connection(endPoint);
                    _connections.Add(con.Token, con);
                    // ReSharper disable once UseNullPropagation
                    if (HandshakeRecieved != null) {
                        HandshakeRecieved.Invoke(con);
                    }
                    return true;
                }
                else if (msg.Header.Type == (short) MessageType.Token) {
                    con = new Connection(endPoint, msg.ConnectionToken);
                    _connections.Add(con.Token, con);
                    return true;
                }
            }
            con = null;
            return false;
        }

        public event Action<Connection> HandshakeRecieved;

        public void InvokeForAllConnections(Action<Connection> method) {
            lock (_connections) {
                foreach (var element in _connections) {
                    method.Invoke(element.Value);
                }
            }
        }
    }
}