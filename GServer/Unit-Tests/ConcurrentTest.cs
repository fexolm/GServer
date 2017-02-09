using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using GServer;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Unit_Tests
{
    public class ServerTest
    {
        [Test]
        public void FirstTest()
        {
            Host host = new Host(8080, 4);
            host.StartListen();
            UdpClient client = new UdpClient(8081);
            var sent=new byte[] { 0, 0, 0, 1 };
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
            client.Send(sent, 4);
            IPEndPoint endPoint = null;
            var received = client.Receive(ref endPoint);
            client.Close();
            host.StopListen();
            Assert.AreEqual(sent.Length, received.Length);
            for (int i = 0; i < sent.Length; i++)
                Assert.AreEqual(sent[i], received[i]);
        }
    }
}
