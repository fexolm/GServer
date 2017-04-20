using GServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Unit_Tests
{
    class PrimitivesTest
    {
        [Test]
        public void MessageCounterTest()
        {
            MessageCounter mc14 = 14;
            MessageCounter mc100 = 100;
            MessageCounter almostMax = short.MaxValue - 100;
            MessageCounter halfMax = short.MaxValue / 2 + 15;
            MessageCounter almostMin = short.MinValue + 100;
            MessageCounter mc15 = 15;
            MessageCounter max = short.MaxValue;
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
        public void CustomListTest()
        {
            CustomList<int> myList = new CustomList<int>();
            List<int> defaultList = new List<int>();
            Random rnd = new Random();
            for (int i = 0; i < 10; i++)
            {
                var val = rnd.Next();
                defaultList.Add(val);
                myList.PushBack(val);
            }

            var mIter = myList.GetEnumerator();
            var dIter = defaultList.GetEnumerator();
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(mIter.MoveNext(), dIter.MoveNext());
                Assert.AreEqual(dIter.Current, mIter.Current);
            }
            Assert.AreEqual(false, mIter.MoveNext());
        }

        [Test]
        public void AckTest()
        {
            Ack sender = new Ack();
            Ack receiver = new Ack();
            var msg = new Message(123, Mode.Reliable);
            Random rnd = new Random();

            List<MessageCounter> sendedMessages = new List<MessageCounter>();
            List<MessageCounter> recievedMessages = new List<MessageCounter>();

            sender.MessageArrived += (mc, type) =>
            {
                if (!recievedMessages.Contains(mc))
                    recievedMessages.Add(mc);
            };

            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    msg.MessageId = (short)rnd.Next(0, 10000);
                    if (!sendedMessages.Contains(msg.MessageId))
                        sendedMessages.Add(msg.MessageId);
                    receiver.ReceiveReliable(msg);
                }
                var acks = receiver.GetAcks();
                foreach (var ack in acks)
                {
                    sender.ProcessReceivedAckBitfield(ack.Val2, ack.Val1, 123);
                }
                recievedMessages.Sort();
                sendedMessages.Sort();
                var rErr1 = recievedMessages.Where(m => !sendedMessages.Contains(m)).ToArray();
                var rErr2 = sendedMessages.Where(m => !recievedMessages.Contains(m)).ToArray();
                Assert.AreEqual(0, rErr1.Length);
                Assert.AreEqual(0, rErr2.Length);
            }

            recievedMessages.Sort();
            sendedMessages.Sort();
            Assert.AreEqual(recievedMessages.Count, sendedMessages.Count);
            for (int i = 0; i < recievedMessages.Count; i++)
            {
                Assert.AreEqual(recievedMessages[i], sendedMessages[i]);
            }


            //Message msg = new Message(123, Mode.Reliable);
            //for(int i=0; i<32; i++)
            //{
            //    msg.MessageId = i;
            //    ack.ReceiveReliable(msg);
            //}

            //for (int i = 32; i < 64; i++)
            //{
            //    msg.MessageId = i;
            //    ack.ReceiveReliable(msg);
            //}

            //var buffer = ack.GetAcks();
            //Assert.AreEqual(2, buffer.Count());
            //Assert.AreEqual(-1, buffer.ToList()[0].Val2);
            //Assert.AreEqual(-1, buffer.ToList()[1].Val2);




        }
    }
}
