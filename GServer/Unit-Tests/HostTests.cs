using System;
using GServer;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using GServer.Messages;

namespace Unit_Tests
{
    internal class HostTests
    {
        [Test]
        [Timeout(8000)]
        public void HostConversationAck() {
            var h1 = new Host(8080);
            var h2 = new Host(8081);
            var err = string.Empty;
            h1.DebugLog = s => { };
            h2.DebugLog = s => { };
            var ts1 = new TestSocketRnd();
            var ts2 = new TestSocketRnd();

            h1.StartListen(ts1);
            h2.StartListen(ts2);
            Thread.Sleep(1000);
            TestSocket.Join(ts1, ts2);
            var successMessage = false;
            var successAck = false;
            var t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            h2.AddHandler((short) MessageType.Ack, (m, e) => { successAck = true; });
            h1.AddHandler(123, (m, e) => { successMessage = true; });
            var connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected) {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message(123, Mode.Reliable));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);
            Assert.AreEqual(true, successMessage, "Сообщение не пришло");
            Assert.AreEqual(true, successAck, "Ack не пришел");

            h1.StopListen();
            h2.StopListen();
        }

        [Test]
        [Timeout(8000)]
        public void HostConversationSequenced() {
            var h1 = new Host(8080);
            var h2 = new Host(8081);
            var err = string.Empty;
            h1.DebugLog = s => { };
            h2.DebugLog = s => { };
            h1.StartListen();
            h2.StartListen();
            Thread.Sleep(1000);
            //TestSocket.Join(ts1, ts2);
            var h1Messages = new List<Message>();
            h1.AddHandler(123, (m, e) => {
                lock (h1Messages) {
                    h1Messages.Add(m);
                }
            });
            var t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            var connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected) {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message(123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message(123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message(123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message(123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message(123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message(123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message(123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message(123, Mode.Reliable | Mode.Sequenced));
            h2.Send(new Message(123, Mode.Reliable | Mode.Sequenced));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);

            Assert.AreEqual(h1Messages.Count, 9, "Сообщение не пришло");
            h1.StopListen();
            h2.StopListen();
            foreach (var connection in h1.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount);
            }
            foreach (var connection in h2.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount);
            }
        }

        [Test]
        [Timeout(8000)]
        public void HostConversationUnreliable() {
            var h1 = new Host(8080);
            var h2 = new Host(8081);
            var err = string.Empty;
            h1.DebugLog = s => { };
            h2.DebugLog = s => { };
            h1.StartListen();
            h2.StartListen();
            Thread.Sleep(1000);
            //TestSocket.Join(ts1, ts2);
            var h1Messages = new List<Message>();
            h1.AddHandler(123, (m, e) => {
                lock (h1Messages) {
                    h1Messages.Add(m);
                }
            });
            var t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            var connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected) {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message(123, Mode.None));
            h2.Send(new Message(123, Mode.None));
            h2.Send(new Message(123, Mode.None));
            h2.Send(new Message(123, Mode.None));
            h2.Send(new Message(123, Mode.None));
            h2.Send(new Message(123, Mode.None));
            h2.Send(new Message(123, Mode.None));
            h2.Send(new Message(123, Mode.None));
            h2.Send(new Message(123, Mode.None));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);

            Assert.AreEqual(9, h1Messages.Count, "Сообщение не пришло");
            h1.StopListen();
            h2.StopListen();
            foreach (var connection in h1.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount);
            }
            foreach (var connection in h2.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount);
            }
        }

        [Test]
        [Timeout(8000)]
        public void HostConversationReliable() {
            var h1 = new Host(8080);
            var h2 = new Host(8081);
            var err = string.Empty;
            h1.DebugLog = s => { };
            h2.DebugLog = s => { };
            h1.StartListen();
            h2.StartListen();
            Thread.Sleep(1000);
            //TestSocket.Join(ts1, ts2);
            var h1Messages = new List<Message>();
            h1.AddHandler(123, (m, e) => {
                lock (h1Messages) {
                    h1Messages.Add(m);
                }
            });
            var t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            var connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected) {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message(123, Mode.Reliable));
            h2.Send(new Message(123, Mode.Reliable));
            h2.Send(new Message(123, Mode.Reliable));
            h2.Send(new Message(123, Mode.Reliable));
            h2.Send(new Message(123, Mode.Reliable));
            h2.Send(new Message(123, Mode.Reliable));
            h2.Send(new Message(123, Mode.Reliable));
            h2.Send(new Message(123, Mode.Reliable));
            h2.Send(new Message(123, Mode.Reliable));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);

            Assert.AreEqual(h1Messages.Count, 9, "Сообщение не пришло");
            h1.StopListen();
            h2.StopListen();
            foreach (var connection in h1.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount);
            }
            foreach (var connection in h2.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount);
            }
        }

        [Test]
        public void HostConversationReliableOrdered() {
            var h1 = new Host(8080);
            var h2 = new Host(8081);
            var err = string.Empty;
            h1.DebugLog = s => { };
            h2.DebugLog = s => { };
            h1.StartListen();
            h2.StartListen();
            Thread.Sleep(1000);
            //TestSocket.Join(ts1, ts2);
            var h1Messages = new List<Message>();
            h1.AddHandler(123, (m, e) => {
                lock (h1Messages) {
                    h1Messages.Add(m);
                }
            });
            var t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(100, 100);
            var connected = false;
            h2.OnConnect = () => { connected = true; };
            while (!connected) {
                h2.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Thread.Sleep(1000);
            }
            h2.Send(new Message(123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message(123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message(123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message(123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message(123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message(123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message(123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message(123, Mode.Reliable | Mode.Ordered));
            h2.Send(new Message(123, Mode.Reliable | Mode.Ordered));
            Thread.Sleep(4000);
            Assert.AreEqual(string.Empty, err);

            Assert.AreEqual(h1Messages.Count, 9, "Сообщение не пришло");
            h1.StopListen();
            h2.StopListen();

            foreach (var connection in h1.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount);
            }
            foreach (var connection in h2.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount);
            }
        }

        [Test]
        public void OrderedIfPacketsLost() {
            ActionDispatcher.Start(1);
            var server = new Host(8080);
            var client = new Host(8081);
            var messageCount = 0;
            MessageCounter lastMsg = 0;
            server.AddHandler(1023, (m, e) => {
                Assert.AreEqual(lastMsg, m.MessageId);
                lastMsg++;
                messageCount++;
            });
            var connected = false;
            client.OnConnect = () => { connected = true; };
            var ts1 = new TestSocketRnd(0.85);
            var ts2 = new TestSocketRnd(0.85);
            server.StartListen(ts1);
            client.StartListen(ts2);
            var t1 = new Timer((o) => ServerTimer.Tick());
            t1.Change(10, 10);
            Thread.Sleep(1000);
            TestSocket.Join(ts1, ts2);
            while (!connected) {
                client.BeginConnect(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080));
                Thread.Sleep(1000);
            }
            for (var i = 0; i < 500; i++) {
                ActionDispatcher.Enqueue(() => {
                    client.Send(new Message(1023, Mode.Reliable | Mode.Ordered, new byte[100]));
                });
            }
            Thread.Sleep(5000);
            Assert.AreEqual(messageCount, 500);
            foreach (var connection in client.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount, "client buffer not empty");
            }
            foreach (var connection in server.GetConnections()) {
                Assert.AreEqual(0, connection.BufferCount, "server buffer not empty");
            }
            ActionDispatcher.Stop();
        }
    }
}