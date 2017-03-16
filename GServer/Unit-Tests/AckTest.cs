using GServer;
using NUnit.Framework;
using System.Collections.Generic;

namespace Unit_Tests
{
    public class AckTest
    {
        [Test]
        public void IfNoLostPackages()
        {
            Ack receiver = new Ack();
            Ack sender = new Ack();
            List<Message> lostMessages = new List<Message>();
            sender.PacketLost += (m) =>
            {
                lostMessages.Add(m);
            };
            Message msg = new Message(1, Mode.Reliable | Mode.Ordered);
            for (MessageCounter mc = 0; mc < 32; mc++)
            {
                msg.MessageId = mc;
                sender.StoreReliable(msg);
                int stat = receiver.ReceiveReliable(msg);
                Assert.AreEqual(-1, stat);
                sender.ProcessReceivedAckBitfield(stat, msg.MessageId);
            }
            Assert.AreEqual(0, lostMessages.Count);
        }
        [Test]
        public void IfSomePackageNotEvenSent()
        {
            Ack receiver = new Ack();
            Ack sender = new Ack();
            List<Message> lostMessages = new List<Message>();
            sender.PacketLost += (m) =>
            {
                lostMessages.Add(m);
            };
            Message msg = new Message(1, Mode.Reliable | Mode.Ordered);
            for (MessageCounter mc = 0; mc < 16; mc++)
            {
                msg.MessageId = mc;
                sender.StoreReliable(msg);
                int stat = receiver.ReceiveReliable(msg);
                Assert.AreEqual(-1, stat);
                sender.ProcessReceivedAckBitfield(stat, msg.MessageId);
            }
            int i = 0;
            msg.MessageId = 16;
            sender.StoreReliable(msg);
            for (MessageCounter mc = 17; mc < 32; mc++)
            {
                msg.MessageId = mc;
                sender.StoreReliable(msg);
                int stat = receiver.ReceiveReliable(msg);
                Assert.AreEqual((~(1 << ++i)), stat);
                sender.ProcessReceivedAckBitfield(stat, msg.MessageId);
            }
            Assert.AreEqual(0, lostMessages.Count);
        }
        [Test]
        public void IfSomePackageLost()
        {
            int stat;
            Ack receiver = new Ack();
            Ack sender = new Ack();
            List<Message> lostMessages = new List<Message>();
            sender.PacketLost += (m) =>
            {
                lostMessages.Add(m);
            };
            Message msg = new Message(1, Mode.Reliable | Mode.Ordered);
            for (MessageCounter mc = 0; mc < 16; mc++)
            {
                msg.MessageId = mc;
                sender.StoreReliable(msg);
                stat = receiver.ReceiveReliable(msg);
                Assert.AreEqual(-1, stat);
                sender.ProcessReceivedAckBitfield(stat, msg.MessageId);
            }
            msg.MessageId = 16;
            sender.StoreReliable(msg);
            int i = 0;
            for (MessageCounter mc = 17; mc < 17 + 31; mc++)
            {
                msg.MessageId = mc;
                sender.StoreReliable(msg);
                stat = receiver.ReceiveReliable(msg);
                Assert.AreEqual((~(1 << ++i)), stat);
                sender.ProcessReceivedAckBitfield(stat, msg.MessageId);
            }
            MessageCounter lostCounter = 17 + 31;
            msg.MessageId = lostCounter;
            sender.StoreReliable(msg);
            stat = receiver.ReceiveReliable(msg);
            Assert.AreEqual(-1, stat);
            sender.ProcessReceivedAckBitfield(stat, msg.MessageId);
            Assert.AreEqual(1, lostMessages.Count);
        }
    }
}
