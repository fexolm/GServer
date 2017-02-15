using GServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var host = new Host(8080);
            var po = new PrivateObject(host);
            var token = Token.GenerateToken();
            var dm = new Datagram(new Message(MessageType.Ack, Mode.Reliable | Mode.Sequenced, token, 123, null).Serialize(), null);
            host.AddHandler((short)MessageType.Ack, (m, e) => 
            {
                msg = m;
            });
            po.Invoke("ProcessDatagram", dm);
            Assert.AreEqual(msg.Header.Type, MessageType.Ack);
            Assert.AreEqual(msg.Header.Reliable, true);
            Assert.AreEqual(msg.Header.Sequenced, true);
            Assert.AreEqual(msg.Header.Ordered, false);
            Assert.AreEqual(msg.Header.MessageId, 123);
            Assert.AreEqual(msg.Body, null);
            Assert.AreEqual(token, msg.Header.ConnectionToken);
        }
    }
}
