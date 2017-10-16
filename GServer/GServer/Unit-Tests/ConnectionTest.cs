using GServer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using GServer.Connection;
using GServer.Messages;

namespace Unit_Tests
{
    class ConnectionTest
    {
        [Test]
        public void ReliableAlghoritmTest() {
            var con = new Connection(null);
            var msg = new Message(123, Mode.Reliable);

            var rnd = new Random();

            var arrivedMessages = new List<MessageCounter>();

            for (var i = 0; i < 10000; i++) {
                msg.MessageId = (short) rnd.Next(0, 10000);
                if (arrivedMessages.Contains(msg.MessageId)) {
                    Assert.AreEqual(true, con.HasAlreadyArrived(msg), i.ToString());
                }
                else {
                    Assert.AreEqual(false, con.HasAlreadyArrived(msg), i.ToString());
                    arrivedMessages.Add(msg.MessageId);
                }
            }
        }
    }
}