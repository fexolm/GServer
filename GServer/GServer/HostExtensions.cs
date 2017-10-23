using GServer.Messages;
using System.Linq;

namespace GServer
{
    public static class HostExtensions
    {
        public static void Ping(this Host host)
        {
            try
            {
                foreach (var connection in host.GetConnections())
                {
                    if (connection.EndPoint != null)
                    {
                        System.Console.WriteLine($"pinging {connection.EndPoint}");
                        host.Send(new Message((short)MessageType.Ping, Mode.Reliable), connection);
                    }
                }
            }
            catch
            {
                System.Console.WriteLine("kek");
            }
        }
    }
}