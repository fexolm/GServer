using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    }
}
