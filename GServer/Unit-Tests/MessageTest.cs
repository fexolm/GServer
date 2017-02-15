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
        public void HandshakeTest()
        {
            var msg = Message.Handshake;
            var bytes = msg.Serialize();
            var dmsg = Message.Deserialize(bytes);

            Assert.AreEqual(dmsg.Header.Ordered, msg.Header.Ordered);
            Assert.AreEqual(dmsg.Header.Reliable, msg.Header.Reliable);
            Assert.AreEqual(dmsg.Header.Sequenced, msg.Header.Sequenced);
            Assert.AreEqual(dmsg.Header.Type, msg.Header.Type);
        }
    }
}
