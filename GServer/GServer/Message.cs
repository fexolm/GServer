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
        public bool Reliable { get; private set; }
        public bool Sequensed { get; private set; }
        public bool Ordered { get; private set; }
        public Token ConnectionToken { get; private set; }
        public int MessageId { get; set; }
        public Int16 TypeId { get; set; }
        public Header(){ }
        public Header(MessageType type, byte mode)
        {
            Type = type;                        
            BitArray Mode = new BitArray((byte)mode);            
            Reliable = Mode.Get(7);
            Sequensed = Mode.Get(6);
            Ordered = Mode.Get(5);

        }        
        public byte[] Serialize()
        {
            BitArray Mode = new BitArray(8);
            Mode.Set(7, Reliable);
            Mode.Set(6, Sequensed);
            Mode.Set(5, Ordered);
            byte[] mode = new byte[1];
            Mode.CopyTo(mode, 0);
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    
                    writer.Write(ConnectionToken.Serialize());
                    writer.Write((byte)Type);
                    writer.Write(mode);
                    if (Mode.Get(7))
                    {
                        writer.Write((byte)MessageId);
                    }
                    if (Mode.Get(6))
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
            byte mode = new byte();
            BitArray Mode = new BitArray(mode);            
            using (MemoryStream m = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.ConnectionToken = new Token(reader.ReadString());                    
                    result.Type = (MessageType)reader.ReadByte();
                    mode = reader.ReadByte();
                    result.Reliable = Mode.Get(7);
                    result.Sequensed = Mode.Get(6);
                    result.Ordered = Mode.Get(5);
                    if(Mode.Get(7))
                    {
                        result.MessageId = reader.ReadInt32();
                    }
                    if (Mode.Get(6))
                        result.TypeId = reader.ReadInt16();                    
                }
            }
            return result;
        }
    }


    public class Message : ISerializable
    {
        public Header Header { get; private set; }
        public byte[] Body { get; private set; }
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(Header.Serialize());
                    if(Body != null)
                    writer.Write(Body);
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
                    result.Header = (Header.Deserialize(buffer));
                    if (reader.PeekChar() == -1)
                        result.Body = null;
                    else
                    {
                        List<byte> bytes = new List<byte>();
                        while (reader.PeekChar() != -1)
                            bytes.Add(reader.ReadByte());
                        result.Body = bytes.ToArray();
                    }
                }
            }
            return result;
        }
        public Message(MessageType Type, byte Mode, ISerializable body)
        {
            Header = new Header(Type, Mode);
            if (body != null)
                Body = body.Serialize();
            else
                Body = null;              
        }
        public Message() { }
    }
}
