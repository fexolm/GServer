//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace GServer
//{
//    class DefaultBehaviour
//    {                case MessageType.Handshake:
//                    var connection = new Connection(datagram.EndPoint);
//                    lock (_connectionManager)
//                    {
//                        _connectionManager.Add(connection.Token, connection);
//                    }
//                    break;
//                case MessageType.Ping:
//                    Connection con = null;
//                    lock (_connectionManager)
//                    {
//                        con = _connectionManager[msg.Header.ConnectionToken];
//                    }
//                    lock (con)
//                    {
//                        con.UpdateActivity();
//                    }
//                    break;

//    }
//}
