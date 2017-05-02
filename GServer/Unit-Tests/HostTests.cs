﻿using GServer;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Unit_Tests
{
    class HostTests
    {
        [Test]
        [Timeout(8000)]
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

            h1.StartListen(10, ts1);
            h2.StartListen(10, ts2);
            Thread.Sleep(1000);
            TestSocket.Join(ts1, ts2);
            bool successMessage = false;
            bool successAck = false;
            Timer t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            h2.AddHandler((short)MessageType.Ack, (m, e) =>
            {
                successAck = true;
            });
            h1.AddHandler((short)123, (m, e) =>
            {
                successMessage = true;
            });
            bool connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected)
            {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message((short)123, Mode.Reliable));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);
            Assert.AreEqual(true, successMessage, "Сообщение не пришло");
            Assert.AreEqual(true, successAck, "Ack не пришел");

            h1.StopListen();
            h2.StopListen();
        }
        [Test]
        [Timeout(8000)]
        public void HostConversationSequenced()
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
            h1.StartListen(0);
            h2.StartListen(0);
            Thread.Sleep(1000);
            //TestSocket.Join(ts1, ts2);
            List<Message> h2Messages = new List<Message>();
            List<Message> h1Messages = new List<Message>();
            h2.AddHandler((short)MessageType.Ack, (m, e) =>
            {
                lock (h2Messages)
                {
                    h2Messages.Add(m);
                }
            });
            h1.AddHandler((short)123, (m, e) =>
            {
                lock (h1Messages)
                {
                    h1Messages.Add(m);
                }
            });
            Timer t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            bool connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected)
            {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Sequenced));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);

            Assert.AreEqual(h1Messages.Count, 9, "Сообщение не пришло");
            h1.StopListen();
            h2.StopListen();
            foreach (var connection in h1.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount);
            }
            foreach (var connection in h2.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount);
            }
        }
        [Test]
        [Timeout(8000)]
        public void HostConversationUnreliable()
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
            h1.StartListen(0);
            h2.StartListen(0);
            Thread.Sleep(1000);
            //TestSocket.Join(ts1, ts2);
            List<Message> h2Messages = new List<Message>();
            List<Message> h1Messages = new List<Message>();
            h2.AddHandler((short)MessageType.Ack, (m, e) =>
            {
                lock (h2Messages)
                {
                    h2Messages.Add(m);
                }
            });
            h1.AddHandler((short)123, (m, e) =>
            {
                lock (h1Messages)
                {
                    h1Messages.Add(m);
                }
            });
            Timer t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            bool connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected)
            {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message((short)123, Mode.None));
            h2.Send(new Message((short)123, Mode.None));
            h2.Send(new Message((short)123, Mode.None));
            h2.Send(new Message((short)123, Mode.None));
            h2.Send(new Message((short)123, Mode.None));
            h2.Send(new Message((short)123, Mode.None));
            h2.Send(new Message((short)123, Mode.None));
            h2.Send(new Message((short)123, Mode.None));
            h2.Send(new Message((short)123, Mode.None));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);

            Assert.AreEqual(9, h1Messages.Count, "Сообщение не пришло");
            h1.StopListen();
            h2.StopListen();
            foreach (var connection in h1.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount);
            }
            foreach (var connection in h2.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount);
            }
        }
        [Test]
        [Timeout(8000)]
        public void HostConversationReliable()
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
            h1.StartListen(0);
            h2.StartListen(0);
            Thread.Sleep(1000);
            //TestSocket.Join(ts1, ts2);
            List<Message> h2Messages = new List<Message>();
            List<Message> h1Messages = new List<Message>();
            h2.AddHandler((short)MessageType.Ack, (m, e) =>
            {
                lock (h2Messages)
                {
                    h2Messages.Add(m);
                }
            });
            h1.AddHandler((short)123, (m, e) =>
            {
                lock (h1Messages)
                {
                    h1Messages.Add(m);
                }
            });
            Timer t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            bool connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected)
            {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message((short)123, Mode.Reliable));
            h2.Send(new Message((short)123, Mode.Reliable));
            h2.Send(new Message((short)123, Mode.Reliable));
            h2.Send(new Message((short)123, Mode.Reliable));
            h2.Send(new Message((short)123, Mode.Reliable));
            h2.Send(new Message((short)123, Mode.Reliable));
            h2.Send(new Message((short)123, Mode.Reliable));
            h2.Send(new Message((short)123, Mode.Reliable));
            h2.Send(new Message((short)123, Mode.Reliable));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);

            Assert.AreEqual(h1Messages.Count, 9, "Сообщение не пришло");
            h1.StopListen();
            h2.StopListen();
            foreach(var connection in h1.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount);
            }
            foreach (var connection in h2.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount);
            }
        }
        [Test]
        public void HostConversationReliableOrdered()
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
            h1.StartListen(0);
            h2.StartListen(0);
            Thread.Sleep(1000);
            //TestSocket.Join(ts1, ts2);
            List<Message> h2Messages = new List<Message>();
            List<Message> h1Messages = new List<Message>();
            h2.AddHandler((short)MessageType.Ack, (m, e) =>
            {
                lock (h2Messages)
                {
                    h2Messages.Add(m);
                }
            });
            h1.AddHandler((short)123, (m, e) =>
            {
                lock (h1Messages)
                {
                    h1Messages.Add(m);
                }
            });
            Timer t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            bool connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected)
            {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message((short)123, Mode.Reliable | Mode.Ordered));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);

            Assert.AreEqual(h1Messages.Count, 9, "Сообщение не пришло");
            h1.StopListen();
            h2.StopListen();

            foreach (var connection in h1.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount);
            }
            foreach (var connection in h2.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount);
            }
        }
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
            var ts1 = new TestSocketRnd(0.85);
            var ts2 = new TestSocketRnd(0.85);
            server.StartListen(0, ts1);
            client.StartListen(0, ts2);
            Timer t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(10, 10);
            Thread.Sleep(1000);
            TestSocket.Join(ts1, ts2);
            while (!connected)
            {
                client.BeginConnect(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
                Thread.Sleep(1000);
            }
            for (int i = 0; i < 500; i++)
            {
                client.Send(new Message(1023, Mode.Reliable | Mode.Ordered, new byte[100]));
            }
            Thread.Sleep(5000);
            Assert.AreEqual(messageCount, 500);
            foreach (var connection in client.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount, "client buffer not empty");
            }
            foreach (var connection in server.GetConnections())
            {
                Assert.AreEqual(0, connection.BufferCount, "server buffer not empty");
            }
        }
    }
}
