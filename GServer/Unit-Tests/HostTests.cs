using GServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Unit_Tests
{
    class HostTests
    {
        [Test]
        public void CorrectThreadCount()
        {
            var po = new PrivateObject(new Host(8080));
            var proc = (List<Thread>)po.GetField("_processingThreads");
            po.Invoke("StartListen", 4);
            Assert.AreEqual(4, proc.Count);
            Thread.Sleep(1000);
            po.Invoke("StopListen");
            Assert.AreEqual(0, proc.Count);
        }
        [Test]
        public void DequeueWithMultipleThreads()
        {
            var po = new PrivateObject(new Host(8080));
            po.Invoke("StartListen", 4);
            var queue = (Queue<Datagram>)po.GetField("_datagrams");
            lock (queue)
            {
                queue.Enqueue(new Datagram(new byte[] { }, null));
                queue.Enqueue(new Datagram(new byte[] { }, null));
            }
            Thread.Sleep(1000);
            lock (queue)
            {
                Assert.AreEqual(0, queue.Count);
            }
            po.Invoke("StopListen");
            lock (queue)
            {
                queue.Enqueue(new Datagram(new byte[] { }, null));
                queue.Enqueue(new Datagram(new byte[] { }, null));
            }
            Thread.Sleep(1000);
            lock (queue)
            {
                Assert.AreEqual(2, queue.Count);
            }
        }
        //[Test]
        //public void DequeueWithNetworkThread()
        //{
        //    var po = new PrivateObject(new Host(8080));
        //    po.Invoke("StartListen", 0);
        //    var queue = (Queue<Datagram>)po.GetField("_datagrams");
        //    lock (queue)
        //    {
        //        queue.Enqueue(new Datagram(new byte[] { }, null));
        //        queue.Enqueue(new Datagram(new byte[] { }, null));
        //    }
        //    Thread.Sleep(1000);
        //    lock (queue)
        //    {
        //        Assert.AreEqual(0, queue.Count);
        //    }
        //    po.Invoke("StopListen");
        //    lock (queue)
        //    {
        //        queue.Enqueue(new Datagram(new byte[] { }, null));
        //        queue.Enqueue(new Datagram(new byte[] { }, null));
        //    }
        //    Thread.Sleep(1000);
        //    lock (queue)
        //    {
        //        Assert.AreEqual(2, queue.Count);
        //    }
        //}
        [Test]
        public void ConnectionRemoveNotActive()
        {
            var con = new Connection(null);
            var pcon = new PrivateObject(con);
            var manager = new ConnectionManager();
            var pmanager = new PrivateObject(manager);

            manager.Add(Token.GenerateToken(), con);
            var dic = (IDictionary<Token, Connection>)pmanager.GetField("_connections");
            Assert.AreEqual(1, dic.Count);
            manager.RemoveNotActive();
            Assert.AreEqual(1, dic.Count);
            pcon.SetProperty("LastActivity", DateTime.Now - TimeSpan.FromSeconds(31));
            manager.RemoveNotActive();
            Assert.AreEqual(0, dic.Count);
        }
        [Test]
        public void DatagramProcessing()
        {
            Message msg = null;
            var host = new Host(8090);
            host.StartListen(0);
            var po = new PrivateObject(host);
            var cm = (ConnectionManager)po.GetField("_connectionManager");
            Connection con = new Connection(null);
            cm.Add(con.Token, con);

            var dm = new Datagram(new Message(MessageType.Ack, Mode.Reliable | Mode.Sequenced, con.Token, 123, null).Serialize(), new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8091));
            host.AddHandler((short)MessageType.Ack, (m, e) =>
            {
                msg = m;
            });
            po.Invoke("ProcessDatagram", dm);
            Assert.AreEqual(msg.Header.Type, MessageType.Ack, "Неверный тип сообщения");
            Assert.AreEqual(msg.Header.Reliable, true, "Пришло не Reliable");
            Assert.AreEqual(msg.Header.Sequenced, true, "Пришло не Sequenced");
            Assert.AreEqual(msg.Header.Ordered, false, "Пришло Ordered");
            Assert.AreEqual(msg.Header.MessageId, 123);
            Assert.AreEqual(msg.Header.ConnectionToken, con.Token);
            host.StopListen();
        }
        [Test]
        public void HostConversation()
        {
            Host h1 = new Host(8080);
            Host h2 = new Host(8081);
            string err = string.Empty;
            string debug = string.Empty;
            h2.ErrLog = s => err += s + "\n";
            h1.DebugLog = s => debug += s + '\n';
            h2.DebugLog = s => debug += s + '\n';
            h1.StartListen(4);
            h2.StartListen(4);
            bool successMessage = false;
            bool successArc = false;
            h2.AddHandler((short)MessageType.Ack, (m, e) => { successArc = true; });
            h1.AddHandler((short)MessageType.Rpc, (m, e) => { successMessage = true; });
            h2.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
            h2.Send(new Message(MessageType.Rpc, Mode.Reliable, null, 123, null));
            Thread.Sleep(4000);

            
            Assert.AreEqual(string.Empty, err);
            Assert.AreEqual(true, successMessage, "Сообщение не пришло");
            Assert.AreEqual(true, successArc, "Arc не пришел");

            h1.StopListen();
            h2.StopListen();
        }
    }
}
