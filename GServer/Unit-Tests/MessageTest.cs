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
            Assert.AreEqual(mesBeforeSer.Header.Type, Message.Deserialize(messageBuffer).Header.Type);
            Assert.AreEqual(mesBeforeSer.Header.Reliable, Message.Deserialize(messageBuffer).Header.Reliable);
            Assert.AreEqual(mesBeforeSer.Header.Sequensed, Message.Deserialize(messageBuffer).Header.Sequensed);
            Assert.AreEqual(mesBeforeSer.Header.Ordered, Message.Deserialize(messageBuffer).Header.Ordered);
            Assert.AreEqual(mesBeforeSer.Header.MessageId, Message.Deserialize(messageBuffer).Header.MessageId);
            Assert.AreEqual(mesBeforeSer.Header.TypeId, Message.Deserialize(messageBuffer).Header.TypeId);
        }
       

    }
}
