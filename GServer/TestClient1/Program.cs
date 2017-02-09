using GServer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestClient1
{
    class Program
    {
        static void Main(string[] args)
        {
            Host host = new Host(8080, 4);
            host.StartListen();
            UdpClient client = new UdpClient(8081);
            var sent = new byte[] { 0, 0, 0, 1 };
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
            client.Send(sent, 4);
            IPEndPoint endPoint = null;
            var received = client.Receive(ref endPoint);
            foreach (var element in received)
            {
                Console.WriteLine(element);
            }
            client.Close();
            host.StopListen();
        }
    }
}
