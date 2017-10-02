using GServer.Messages;

namespace GServer
{
    public static class HostExtensions
    {
        public static void Ping(this Host host) {
            foreach (var connection in host.GetConnections()) {
                host.Send(new Message((short) MessageType.Ping, Mode.Reliable), connection);
            }
        }
    }
}