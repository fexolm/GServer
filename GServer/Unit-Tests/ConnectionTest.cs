using GServer;
using NUnit.Framework;

namespace Unit_Tests
{
    class ConnectionTest
    {
        [Test]
        public void ReliableAlghoritmTest()
        {
            Connection con = new Connection(null);
            Message msg = new Message(123, Mode.Reliable);
            msg.MessageId = 1;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 3;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 7;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 8;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 0;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 7;
            Assert.AreEqual(true, con.HasAlreadyArrived(msg));
            msg.MessageId = 40;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 35;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 29;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 14;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 28;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 27;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 29;
            Assert.AreEqual(true, con.HasAlreadyArrived(msg));
            msg.MessageId = 0;
            Assert.AreEqual(true, con.HasAlreadyArrived(msg));

            msg.MessageId = 4;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 5;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 2;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 6;
            Assert.AreEqual(false, con.HasAlreadyArrived(msg));
            msg.MessageId = 7;
            Assert.AreEqual(true, con.HasAlreadyArrived(msg));
        }
    }
}
