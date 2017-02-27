using NUnit.Framework;
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

        [Test]
        public void SerializationTest()
        {
            var token = Token.GenerateToken();
            var ds = new DataStorage();
            ds.Push(13);
            ds.Push("word");
            ds.Push(true);
            ds.Push("hello world");
            ds.Push(13.221F);
            ds.Push(14.32D);
            var msg = new Message(MessageType.Ack, Mode.Reliable | Mode.Sequenced, ds);
            msg.MessageId = 123;
            msg.ConnectionToken = token;
            var bytes = msg.Serialize();
            var newMsg = Message.Deserialize(bytes);

            Assert.AreEqual(msg.Header.Type, newMsg.Header.Type);
            Assert.AreEqual(msg.Header.MessageId, newMsg.Header.MessageId);
            Assert.AreEqual(msg.Header.Ordered, newMsg.Header.Ordered);
            Assert.AreEqual(msg.Header.Reliable, newMsg.Header.Reliable);
            Assert.AreEqual(msg.Header.Sequenced, newMsg.Header.Sequenced);
            Assert.AreEqual(msg.Header.ConnectionToken, newMsg.Header.ConnectionToken);

            var readDs = new DataStorage(msg.Body);

            Assert.AreEqual(13, readDs.ReadInt32());
            Assert.AreEqual("word", readDs.ReadString());
            Assert.AreEqual(true, readDs.ReadBoolean());
            Assert.AreEqual("hello world", readDs.ReadString());
            Assert.AreEqual(13.221F, readDs.ReadFloat());
            Assert.AreEqual(14.32D, readDs.ReadDouble());
        }
        [Test]
        public void DataStorageTest()
        {
            DataStorage ds = new DataStorage();
            ds.Push(13);
            ds.Push("word");
            ds.Push(true);
            ds.Push("hello world");
            ds.Push(13.221F);
            ds.Push(14.32D);

            DataStorage readDs = new DataStorage(ds.Serialize());

            Assert.AreEqual(13, readDs.ReadInt32());
            Assert.AreEqual("word", readDs.ReadString());
            Assert.AreEqual(true, readDs.ReadBoolean());
            Assert.AreEqual("hello world", readDs.ReadString());
            Assert.AreEqual(13.221F, readDs.ReadFloat());
            Assert.AreEqual(14.32D, readDs.ReadDouble());
        }
    }
}
