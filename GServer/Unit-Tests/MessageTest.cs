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
            byte[] buffer = mesBeforeSer.Serialize();            
            Assert.AreEqual(mesBeforeSer, Message.Deserialize(buffer));
        }
        [Test]
        public void HeaderHendlerTest()
        {

        }

    }
}
