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
    public enum MessageType : byte
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
        Unreliable = 0x0,
        Reliable = 0x1,
        Sequenced = 0x2,
        Ordered = 0x4,
    }

    public class MessageHeader : ISerializable
    {
        #region fields
        public MessageType Type { get; private set; }
        private Mode _mode;
        public Token ConnectionToken { get; private set; }
        public int MessageId { get; private set; }
        #endregion

        public bool Reliable { get { return (_mode & Mode.Reliable) == Mode.Reliable; } }
        public bool Sequenced { get { return (_mode & Mode.Sequenced) == Mode.Sequenced; } }
        public bool Ordered { get { return (_mode & Mode.Ordered) == Mode.Ordered; } }

        private MessageHeader() { }
        public MessageHeader(MessageType type, Mode mode, Token token, int messageId)
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
                    writer.Write((byte)Type);
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
            using (BinaryReader reader = new BinaryReader(m))
            {
                result.Type = (MessageType)reader.ReadByte();
                result._mode = (Mode)reader.ReadByte();
                if (result.Type != MessageType.Empty && result.Type != MessageType.Handshake)
                {
                    result.ConnectionToken = new Token(reader.ReadString());
                    result.MessageId = reader.ReadInt32();
                }
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
                    if (reader.PeekChar() == -1)
                    {
                        result.Body = null;
                    }
                    else
                    {
                        List<byte> bytes = new List<byte>();
                        while (reader.PeekChar() != -1)
                        {
                            bytes.Add(reader.ReadByte());
                        }
                        result.Body = bytes.ToArray();
                    }
                }
            }
            return result;
        }
        public Message(MessageType type, Mode mode, Token token, int messageId, ISerializable body)
        {
            Header = new MessageHeader(type, mode, token, messageId);
            if (body != null)
            {
                Body = body.Serialize();
            }
            else
                Body = null;
        }

        private static readonly Message _handshake = new Message(MessageType.Handshake, Mode.Reliable | Mode.Ordered, null, default(int), null);

        public static Message Handshake { get { return _handshake; } }

        private Message() { }
    }
}
