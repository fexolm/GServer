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
        Empty = 0,
        Handshake = 1,
        Ping = 2,
        Rpc = 3,
        Authorization = 4,
        Ack = 5,
    }

    public enum ModeType : byte
    {
        Unreliable = 0,
        UnreliableSequenced = 2,
        ReliableUnsequenced = 4,
        ReiableSequenced = 6,
        ReliableOrdered = 7
    }
    public class Header : ISerializable
    {
        public MessageType Type { get; private set; }
        public ModeType Mode { get; private set; }
        public Token ConnectionToken { get; private set; }
        public int MessageId { get; set; }
        public Int16 TypeId { get; set; }
        public Header(){ }
        public Header(Host host, MessageType type, ModeType mode)
        {
            Type = type;
            Mode = mode;
            //Token;
            BitArray buffer = new BitArray((byte)Mode);
            if (buffer.Get(3))
                MessageId = host.MessageCount++;
            //if (buffer.Get(2))
            //    TypeId = host.TypeCounts[type]++;
        }        
        public byte[] Serialize()
        {
            BitArray buffer = new BitArray((byte)Mode);
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    
                    writer.Write(ConnectionToken.Serialize());
                    writer.Write((byte)Type);
                    writer.Write((byte)Mode);
                    if (buffer.Get(3))
                    {
                        writer.Write((byte)MessageId);
                    }
                    if (buffer.Get(2))
                    {
                        writer.Write((byte)TypeId);
                    }
                }
                return m.ToArray();
            }
        }
        public static Header Deserialize(byte[] buffer)
        {
            var result = new Header();
            BitArray ModeOfMessage = new BitArray((byte)result.Mode);
            using (MemoryStream m = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.ConnectionToken = new Token(reader.ReadString());                    
                    result.Type = (MessageType)reader.ReadByte();
                    result.Mode = (ModeType)reader.ReadByte();
                    if(ModeOfMessage.Get(3))
                    {
                        result.MessageId = reader.ReadInt16();
                    }                    
                }
            }
            return result;
        }
    }
    public class Message : ISerializable
    {
        public Header Header { get; private set; }
        public ISerializable body { get; private set; }
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(Header.Serialize());
                    writer.Write(body.Serialize());
                }
                return m.ToArray();
            }
        }
        public static Message Deserialize(byte[] buffer)
        {
            throw new NotImplementedException();
        }
        public Message(Host host, MessageType Type, ModeType Mode/*, byte[] Body*/)
        {
            Header = new Header(host, (MessageType)Type, (ModeType)Mode );
            //body = Body;                  
        }
    }
}

