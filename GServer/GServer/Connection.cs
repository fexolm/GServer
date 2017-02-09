using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GServer
{
    public class Connection
    {
        public readonly IPEndPoint EndPoint;
        public readonly Token Token;
        public DateTime LastActivity { get; private set; }
        public Connection(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            Token = Token.GenerateToken();
            LastActivity = DateTime.Now;
        }
        public void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }
        public event Action<Connection> Disconnected;
        public void Disconnect()
        {
            if (Disconnected != null)
                Disconnected.Invoke(this);
        }
    }
}
