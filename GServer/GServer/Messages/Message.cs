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

    public enum Mode : byte
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
        public Header(MessageType type, Mode mode, Token token)
        {
            Type = type;
            ConnectionToken = token;                        
            BitArray Mode = new BitArray(new byte[] { (byte)mode });                        
            Reliable = Mode.Get(0);
            Sequensed = Mode.Get(1);
            Ordered = Mode.Get(2);

        }
        public Header(MessageType type, Mode mode)
        {
            Type = type;
            
            BitArray Mode = new BitArray(new byte[] { (byte)mode}) ;
            Reliable = Mode.Get(0);
            Sequensed = Mode.Get(1);
            Ordered = Mode.Get(2);
        }        
        public byte[] Serialize()
        {
            BitArray Mode = new BitArray(8);
            Mode.Set(0, Reliable);
            Mode.Set(1, Sequensed);
            Mode.Set(2, Ordered);
            byte[] mode = new byte[1];
            Mode.CopyTo(mode, 0);
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {   
                    writer.Write((byte)Type);
                    if(ConnectionToken != null)
                    {
                        writer.Write(ConnectionToken.Serialize());
                    }                    
                    writer.Write(mode);
                    if (Mode.Get(0))
                    {
                        writer.Write((byte)MessageId);
                    }
                    if (Mode.Get(1))
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
            BitArray Mode = new BitArray(new byte[] { mode });            
            using (MemoryStream m = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {                                       
                    result.Type = (MessageType)reader.ReadByte();
                    if(result.Type != MessageType.Handshake && reader.PeekChar() != -1)
                    {
                        result.ConnectionToken = new Token(reader.ReadString());
                    }                     
                    mode = reader.ReadByte();
                    result.Reliable = Mode.Get(0);
                    result.Sequensed = Mode.Get(1);
                    result.Ordered = Mode.Get(2);
                    if(Mode.Get(7) && reader.PeekChar() != -1)
                    {
                        result.MessageId = reader.ReadInt32();
                    }
                    if (Mode.Get(6) && reader.PeekChar() != -1)
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
        public Message(MessageType type, Mode mode, Token token, ISerializable body)
        {
            Header = new Header(type, mode, token);
            if (body != null)
                Body = body.Serialize();
            else
                Body = null;              
        }
        public Message(MessageType type, Mode mode, ISerializable body)
        {
            Header = new Header(type, mode);
            if (body != null)
                Body = body.Serialize();
            else
                Body = null;
        }

        private static readonly Message _handshake = new Message { Header = new Header(), Body = null };

        public static Message Handshake {  get { return _handshake; } }

        
        

        public Message(MessageType type)
        {
            Header = new Header(type, Mode.Unreliable);
            Body = null;
        }

        private Message() { }
    }
}
