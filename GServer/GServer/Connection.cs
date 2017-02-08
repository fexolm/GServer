using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GServer
{
    class Connection
    {
        public readonly IPEndPoint EndPoint;
        public readonly Token Token;
        public Connection(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            Token = Token.GenerateToken();
        }
    }
}
