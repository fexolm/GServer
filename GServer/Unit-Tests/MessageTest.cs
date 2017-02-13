using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System.Text;
using GServer;

namespace Unit_Tests
{
    class MessageTest
    {
        [Test]
        public void SerializationTest()
        {
            var mesBeforeSer = new Message(MessageType.Rpc, Mode.ReliableUnsequenced, null);
            byte[] messageBuffer = mesBeforeSer.Serialize();
            Message Mess = Message.Deserialize(messageBuffer);
            Assert.AreEqual(mesBeforeSer.Header.Type, Mess.Header.Type);
            Assert.AreEqual(mesBeforeSer.Header.Reliable, Mess.Header.Reliable);
            Assert.AreEqual(mesBeforeSer.Header.Sequensed, Mess.Header.Sequensed);
            Assert.AreEqual(mesBeforeSer.Header.Ordered, Mess.Header.Ordered);
            //Assert.AreEqual(mesBeforeSer.Header.MessageId, Mess.Header.MessageId);
            //Assert.AreEqual(mesBeforeSer.Header.TypeId, Mess.Header.TypeId);
        }
       

    }
}
