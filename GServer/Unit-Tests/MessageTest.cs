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
            mesBeforeSer.Header.MessageId = 1;
            mesBeforeSer.Header.TypeId = 1;
            byte[] messageBuffer = mesBeforeSer.Serialize();
            Message Mess = Message.Deserialize(messageBuffer);
            Assert.AreEqual(mesBeforeSer.Header.Type, Mess.Header.Type);
            Assert.AreEqual(mesBeforeSer.Header.Reliable, Mess.Header.Reliable);
            Assert.AreEqual(mesBeforeSer.Header.Sequensed, Mess.Header.Sequensed);
            Assert.AreEqual(mesBeforeSer.Header.Ordered, Mess.Header.Ordered);
            Assert.AreEqual(mesBeforeSer.Header.MessageId, Mess.Header.MessageId);
            Assert.AreEqual(mesBeforeSer.Header.TypeId, Mess.Header.TypeId);
        }
        [Test]
        public void MessageBuilderTest()
        {
            Token token = new Token("token");
            MessageBuilder builder = new MessageBuilder();
            Message testMessage = builder.MessageToSend(MessageType.Handshake, Mode.Unreliable);
            Message rpcMessage1 = builder.MessageToSend(MessageType.Rpc, Mode.ReliableSequenced, token, null);
            Message rpcMessage2 = builder.MessageToSend(MessageType.Rpc, Mode.ReliableSequenced, token, null);
            Message emptyMessage1 = builder.MessageToSend(MessageType.Empty, Mode.ReliableOrdered, token, null);
            Message emptyMessage2 = builder.MessageToSend(MessageType.Empty, Mode.ReliableOrdered, token, null);
            Message emptyMessage3 = builder.MessageToSend(MessageType.Empty, Mode.ReliableOrdered, token, null);
            Assert.AreEqual(builder.allMesssageCount, 7);
            Assert.AreEqual(builder.TypesCountsSend[MessageType.Empty], 3);
            Assert.AreEqual(builder.TypesCountsSend[MessageType.Rpc], 2);
            Assert.AreEqual(emptyMessage2.Header.MessageId, 6);
            Assert.AreEqual(emptyMessage2.Header.TypeId, 2);
            Assert.AreEqual(rpcMessage1.Header.Type, MessageType.Rpc);
            Assert.AreEqual(rpcMessage1.Header.Reliable, true);
            Assert.AreEqual(rpcMessage1.Header.Sequensed, true);
            Assert.AreEqual(rpcMessage1.Header.Ordered, false);
        }
        
        [Test]
        public void ModeMessageHandler()
        {
            Token token = new Token("token");
            MessageBuilder builder = new MessageBuilder();
            Message testMessage1 = builder.MessageToSend(MessageType.Rpc, Mode.ReliableOrdered);
            Message testMessage2 = builder.MessageToSend(MessageType.Rpc, Mode.ReliableOrdered);
            Message testMessage3 = builder.MessageToSend(MessageType.Rpc, Mode.ReliableOrdered);
            ModeMessageHandler handler = new ModeMessageHandler();
            Message testAck1 = handler.HeaderWorker(testMessage1);
            byte[] messageBuffer = testAck1.Serialize();
            Message Mess = Message.Deserialize(messageBuffer);
            Assert.AreEqual(testAck1.Header.Type, MessageType.Ack);
            Assert.AreEqual(testAck1.Header.Reliable, false);
            Assert.AreEqual(testAck1.Header.Sequensed, false);
            Assert.AreEqual(Mess.Body, (byte)1);
            Message testAck2 = handler.HeaderWorker(testMessage3);
            Assert.AreEqual(testAck2, null);
        }
        

    }
}
