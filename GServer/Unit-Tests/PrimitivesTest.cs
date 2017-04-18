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
            Ack ack = new Ack();

            Message msg = new Message(123, Mode.Reliable);
            for(int i=0; i<32; i++)
            {
                msg.MessageId = i;
                ack.ReceiveReliable(msg);
            }

            for (int i = 32; i < 64; i++)
            {
                msg.MessageId = i;
                ack.ReceiveReliable(msg);
            }

            var buffer = ack.GetAcks();
            Assert.AreEqual(2, buffer.Count());
            Assert.AreEqual(-1, buffer.ToList()[0].Val2);
            Assert.AreEqual(-1, buffer.ToList()[1].Val2);
        }
    }
}
