using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Collections;


namespace GServer
{
    public enum MessageType : short
    {
        Empty,
        Handshake,
        Ping,
        Rpc,
        Authorization,
        Ack,
    }

    [Flags]
    public enum Mode : byte
    {
        None = 0x0,
        Reliable = 0x1,
        Sequenced = 0x2,
        Ordered = 0x4,
    }

    public class MessageHeader : ISerializable
    {
        #region fields
        public MessageType Type { get; private set; }
        private Mode _mode;
        public Token ConnectionToken { get; set; }
        public short MessageId { get; private set; }
        #endregion
        public bool Reliable { get { return (_mode & Mode.Reliable) == Mode.Reliable; } }
        public bool Sequenced { get { return (_mode & Mode.Sequenced) == Mode.Sequenced; } }
        public bool Ordered { get { return (_mode & Mode.Ordered) == Mode.Ordered; } }

        private MessageHeader() { }
        public MessageHeader(MessageType type, Mode mode, Token token, short messageId)
        {
            Type = type;
            _mode = mode;
            ConnectionToken = token;
            MessageId = messageId;
        }
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write((short)Type);
                    writer.Write((byte)_mode);
                    if (Type != MessageType.Empty && Type != MessageType.Handshake)
                    {
                        writer.Write(ConnectionToken.Serialize());
                        writer.Write(MessageId);
                    }
                }
                return m.ToArray();
            }
        }
        public static MessageHeader Deserialize(MemoryStream m)
        {
            var result = new MessageHeader();
            BinaryReader reader = new BinaryReader(m);
            result.Type = (MessageType)reader.ReadInt16();
            result._mode = (Mode)reader.ReadByte();
            if (result.Type != MessageType.Empty && result.Type != MessageType.Handshake)
            {
                result.ConnectionToken = new Token(reader.ReadInt32());
                result.MessageId = reader.ReadInt16();
            }
            return result;
        }
    }

    public class Message : ISerializable
    {
        public MessageHeader Header { get; private set; }
        public byte[] Body { get; private set; }
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(Header.Serialize());
                    if (Body != null)
                    {
                        writer.Write(Body);
                    }
                }
                return m.ToArray();
            }
        }
        public static Message Deserialize(byte[] buffer)
        {
            Message result = new Message();
            using (MemoryStream m = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.Header = (MessageHeader.Deserialize(m));
                    result.Body = reader.ReadBytes((int)(m.Length - m.Position));
                }
            }
            return result;
        }
        public Message(MessageType type, Mode mode, Token token, short messageId, ISerializable body)
        {
            Header = new MessageHeader(type, mode, token, messageId);
            if (body != null)
            {
                Body = body.Serialize();
            }
            else
                Body = null;
        }

        private static readonly Message _handshake = new Message(MessageType.Handshake, Mode.Ordered, null, default(int), null);
        public static Message Handshake { get { return _handshake; } }
        public static Message Ack(MessageHeader header, int ackBitField)
        {
            var ds = new DataStorage();
            ds.Push(ackBitField);
            ds.Push((short)header.Type);
            return new Message(MessageType.Ack, Mode.Sequenced, header.ConnectionToken, header.MessageId, ds);
        }
        private Message() { }
    }
}
