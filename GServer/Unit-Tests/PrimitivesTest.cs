using GServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using GServer.Containers;
using GServer.Messages;

namespace Unit_Tests
{
    class PrimitivesTest
    {
        [Test]
        public void MessageCounterTest() {
            MessageCounter mc14 = 14;
            MessageCounter mc100 = 100;
            MessageCounter almostMax = short.MaxValue - 100;
            MessageCounter halfMax = short.MaxValue / 2 + 15;
            MessageCounter almostMin = short.MinValue + 100;
            MessageCounter min = short.MinValue;
            Assert.Greater(mc100, mc14, "100 < 14");
            Assert.Greater(halfMax, mc100, "halfMax < 100");
            Assert.Greater(mc14, halfMax, "14 < halfMax");
            Assert.Greater(almostMax, halfMax, "almostMax < halfMax");
            Assert.Greater(mc100, almostMax, "100 < almostMax");
            Assert.Greater(mc14, almostMax, "14 < almostMax");
            Assert.AreEqual(min - 1, 1);
            Assert.AreEqual(almostMin - 130, 30);
            Assert.AreEqual(mc100 - 100, 0);
        }

        [Test]
        public void CustomListTest() {
            var myList = new CustomList<int>();
            var defaultList = new List<int>();
            var rnd = new Random();
            for (var i = 0; i < 10; i++) {
                var val = rnd.Next();
                defaultList.Add(val);
                myList.PushBack(val);
            }

            var mIter = myList.GetEnumerator();
            var dIter = defaultList.GetEnumerator();
            for (var i = 0; i < 10; i++) {
                Assert.AreEqual(mIter.MoveNext(), dIter.MoveNext());
                Assert.AreEqual(dIter.Current, mIter.Current);
            }
            Assert.AreEqual(false, mIter.MoveNext());
            mIter.Dispose();
            dIter.Dispose();
        }

        [Test]
        public void AckTest() {
            var sender = new Ack();
            var receiver = new Ack();
            var msg = new Message(123, Mode.Reliable);
            var rnd = new Random();

            var sendedMessages = new List<MessageCounter>();
            var recievedMessages = new List<MessageCounter>();

            sender.MessageArrived += (mc, type) => {
                if (!recievedMessages.Contains(mc))
                    recievedMessages.Add(mc);
            };

            for (var i = 0; i < 10; i++) {
                for (var j = 0; j < 100; j++) {
                    msg.MessageId = (short) rnd.Next(0, 10000);
                    if (!sendedMessages.Contains(msg.MessageId))
                        sendedMessages.Add(msg.MessageId);
                    receiver.ReceiveReliable(msg);
                }
                var acks = receiver.GetAcks();
                foreach (var ack in acks) {
                    sender.ProcessReceivedAckBitfield(ack.Val2, ack.Val1, 123);
                }
                var rErr1 = recievedMessages.Where(m => !sendedMessages.Contains(m)).ToArray();
                var rErr2 = sendedMessages.Where(m => !recievedMessages.Contains(m)).ToArray();
                Assert.AreEqual(0, rErr1.Length);
                Assert.AreEqual(0, rErr2.Length);
            }
        }
    }
}