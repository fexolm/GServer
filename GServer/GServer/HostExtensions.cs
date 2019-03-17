using GServer.Messages;
using System.Linq;

namespace GServer
{
    public static class HostExtensions
    {
        public static void Ping(this Host host)
        {
            foreach (var connection in host.GetConnections())
            {
                if (connection.EndPoint == null) continue;
                host.Send(new Message((short)MessageType.Ping, Mode.Reliable), connection);
            }
        }
    }
}
