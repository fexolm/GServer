using GServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Unit_Tests
{
    class ConnectionTest
    {
        [Test]
        public void ReliableAlghoritmTest()
        {
            Connection con = new Connection(null);
            Message msg = new Message(123, Mode.Reliable);

            Random rnd = new Random();

            List<MessageCounter> arrivedMessages = new List<MessageCounter>();

            for (int i = 0; i < 10000; i++)
            {
                msg.MessageId = (short)rnd.Next(0, 10000);
                if (arrivedMessages.Contains(msg.MessageId))
                {
                    Assert.AreEqual(true, con.HasAlreadyArrived(msg), i.ToString());
                }
                else
                {
                    Assert.AreEqual(false, con.HasAlreadyArrived(msg), i.ToString());
                    arrivedMessages.Add(msg.MessageId);
                }
            }
        }
        [Test]
        public void FailedTest()
        {
            Assert.Fail();
        }
    }
}
