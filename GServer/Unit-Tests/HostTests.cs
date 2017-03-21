using GServer;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Unit_Tests
{
    class HostTests
    {
        [Test]
        [Timeout(6000)]
        public void HostConversationAck()
        {
            Host h1 = new Host(8080);
            Host h2 = new Host(8081);
            string err = string.Empty;
            string debug = string.Empty;
            h2.ErrLog = s => err += s + "\n";
            h1.DebugLog = s => debug += s + '\n';
            h2.DebugLog = s => debug += s + '\n';
            var ts1 = new TestSocketRnd();
            var ts2 = new TestSocketRnd();
            h1.StartListen(4, ts1);
            h2.StartListen(0, ts2);
            TestSocket.Join(ts1, ts2);
            bool successMessage = false;
            bool successArc = false;
            h2.AddHandler((short)MessageType.Ack, (m, e) => { successArc = true; });
            h1.AddHandler((short)MessageType.Rpc, (m, e) => { successMessage = true; });
            bool connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected)
            {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable));
            Thread.Sleep(4000);

            Assert.AreEqual(string.Empty, err);
            Assert.AreEqual(true, successMessage, "Сообщение не пришло");
            Assert.AreEqual(true, successArc, "Arc не пришел");

            h1.StopListen();
            h2.StopListen();
        }
        [Test]
        [Timeout(6000)]
        public void HostConversationSequenced()
        {
            Host h1 = new Host(8080);
            Host h2 = new Host(8081);
            string err = string.Empty;
            string debug = string.Empty;
            h2.ErrLog = s => err += s + "\n";
            h1.DebugLog = s => debug += s + ' ';
            h2.DebugLog = s => debug += s + ' ';
            var ts1 = new TestSocketRnd();
            var ts2 = new TestSocketRnd();
            h1.StartListen(48, ts1);
            h2.StartListen(0, ts2);
            TestSocket.Join(ts1, ts2);
            List<Message> h2Messages = new List<Message>();
            List<Message> h1Messages = new List<Message>();
            h2.AddHandler((short)MessageType.Ack, (m, e) =>
            {
                lock (h2Messages)
                {
                    h2Messages.Add(m);
                }
            });
            h1.AddHandler((short)MessageType.Rpc, (m, e) =>
            {
                lock (h1Messages)
                {
                    h1Messages.Add(m);
                }
            });
            bool connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected)
            {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
                Thread.Sleep(1000);
            }

            for (short i = 0; i < 100; i++)
            {
                h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Sequenced));
            }
            Thread.Sleep(1000);

            Assert.AreEqual(string.Empty, err);
            Assert.AreEqual(100, h1Messages.Count, "Сообщение не пришло");
            Assert.AreEqual(100, h2Messages.Count, "Akc не пришел");

            h1.StopListen();
            h2.StopListen();

        }
        [Test]
        [Timeout(6000)]
        public void HostConversationOrdered()
        {
            Host h1 = new Host(8080);
            Host h2 = new Host(8081);
            string err = string.Empty;
            string debug = string.Empty;
            h2.ErrLog = s => err += s + "\n";
            h1.DebugLog = s => debug += s + '\n';
            h2.DebugLog = s => debug += s + '\n';
            var ts1 = new TestSocketRnd();
            var ts2 = new TestSocketRnd();
            h1.StartListen(100, ts1);
            h2.StartListen(1, ts2);
            TestSocket.Join(ts1, ts2);
            List<Message> h2Messages = new List<Message>();
            List<Message> h1Messages = new List<Message>();
            h2.AddHandler((short)MessageType.Ack, (m, e) =>
            {
                lock (h2Messages)
                {
                    h2Messages.Add(m);
                }
            });
            h1.AddHandler((short)MessageType.Rpc, (m, e) =>
            {
                lock (h1Messages)
                {
                    h1Messages.Add(m);
                }
            });
            bool connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected)
            {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));

            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));

            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));

            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));

            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));

            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));

            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));

            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));

            h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));
            Thread.Sleep(2000);
            Assert.AreEqual(string.Empty, err);
            Assert.AreEqual(9, h1Messages.Count, "Сообщение не пришло");
            Assert.AreEqual(9, h2Messages.Count, "Ack не пришел");
            h1.StopListen();
            h2.StopListen();
        }
        //[Test]
        //public void DurationTest()
        //{
        //    Host h1 = new Host(8080);
        //    Host h2 = new Host(8081);
        //    string err = string.Empty;
        //    string debug = string.Empty;
        //    h2.ErrLog = s => err += s + "\n";
        //    h1.DebugLog = s => debug += s + '\n';
        //    h2.DebugLog = s => debug += s + '\n';
        //    h1.StartListen(3);
        //    h2.StartListen(3);
        //    List<Message> h2Messages = new List<Message>();
        //    List<Message> h1Messages = new List<Message>();
        //    h2.AddHandler((short)MessageType.Ack, (m, e) =>
        //    {
        //        lock (h2Messages)
        //        {
        //            h2Messages.Add(m);
        //        }
        //    });
        //    h1.AddHandler((short)MessageType.Rpc, (m, e) =>
        //    {
        //        lock (h1Messages)
        //        {
        //            h1Messages.Add(m);
        //            for (int i = 0; i < 100000; i++)
        //                ;
        //        }
        //    });
        //    bool connected = false;
        //    h2.OnConnect = () => { connected = true; };
        //    h2.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));

        //    while (!connected)
        //        ;

        //    for (short i = 0; i < 10000; i++)
        //    {
        //        h2.Send(new Message((short)MessageType.Rpc, Mode.Reliable | Mode.Ordered));
        //    }
        //    while (!(h1Messages.Count == 10000) || !(h2Messages.Count == 10000))
        //        ;

        //    h1.StopListen();
        //    h2.StopListen();
        //}
        [Test]
        public void OrderedIfPacketsLost()
        {
            Host server = new Host(8080);
            Host client = new Host(8081);
            int messageCount = 0;
            MessageCounter lastMsg = 0;
            server.AddHandler(1023, (m, e) =>
            {
                Assert.AreEqual(lastMsg, m.MessageId);
                lastMsg++;
                messageCount++;
            });
            bool connected = false;
            client.OnConnect = () => { connected = true; };
            var ts1 = new TestSocketRnd(1);
            var ts2 = new TestSocketRnd(0.7);
            server.StartListen(0, ts1);
            client.StartListen(0, ts2);
            TestSocket.Join(ts1, ts2);
            while (!connected)
            {
                client.BeginConnect(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
                Thread.Sleep(1000);
            }

            for (int i = 0; i < 300; i++)
            {
                client.Send(new Message(1023, Mode.Reliable | Mode.Ordered));
            }
            Thread.Sleep(6000);
            Assert.GreaterOrEqual(messageCount, 260);
        }
    }
}
