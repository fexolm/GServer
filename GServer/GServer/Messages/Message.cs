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

    public enum Mode : byte
    {
        Unreliable = 0,
        UnreliableSequenced = 2,
        ReliableUnsequenced = 4,
        ReliableSequenced = 6,
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
        public Header(MessageType _type, Mode _mode, Token _token)
        {
            Type = _type;
            ConnectionToken = _token;                        
            BitArray Mode = new BitArray(new byte[] { (byte)_mode });                        
            Reliable = Mode.Get(0);
            Sequensed = Mode.Get(1);
            Ordered = Mode.Get(2);

        }
        public Header(MessageType _type, Mode _mode)
        {
            Type = _type;
            
            BitArray Mode = new BitArray(new byte[] { (byte)_mode}) ;
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
        public static Header Deserialize(byte[] _buffer)
        {
            var result = new Header();            
                        
            using (MemoryStream m = new MemoryStream(_buffer))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {                                       
                    result.Type = (MessageType)reader.ReadByte();
                    if(result.Type != MessageType.Handshake)
                    {
                        result.ConnectionToken = new Token(reader.ReadString());
                    }                     
                    byte mode = reader.ReadByte();
                    BitArray Mode = new BitArray(new byte[] { mode });
                    result.Reliable = Mode.Get(0);
                    result.Sequensed = Mode.Get(1);
                    result.Ordered = Mode.Get(2);
                    if(Mode.Get(7) && reader.PeekChar() != -1)
                    {
                        result.MessageId = reader.ReadInt32();
                    }
                    if (Mode.Get(6) && reader.PeekChar() != -1)
                    {
                        result.TypeId = reader.ReadInt16();
                    }
                                            
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
                    if (Body != null)
                    {
                        writer.Write(Body);
                    }
                }
                return m.ToArray();
            }
        }
        public static Message Deserialize(byte[] _buffer)
        {
            Message result = new Message();
            using (MemoryStream m = new MemoryStream(_buffer))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.Header = (Header.Deserialize(_buffer));
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
        public Message(MessageType _type, Mode _mode, Token _token, ISerializable _body)
        {
            Header = new Header(_type, _mode, _token);
            if (_body != null)
            {
                Body = _body.Serialize();
            }
            else
                Body = null;              
        }
        public Message(MessageType _type, Mode _mode, ISerializable _body)
        {
            Header = new Header(_type, _mode);
            if (_body != null)
            {
                Body = _body.Serialize();
            }
            else
                Body = null;
        }

        private static readonly Message _handshake = new Message { Header = new Header(MessageType.Handshake, Mode.Unreliable), Body = null };

        public static Message Handshake {  get { return _handshake; } }

        
        

        public Message(MessageType _type)
        {
            Header = new Header(_type, Mode.Unreliable);
            Body = null;
        }

        private Message() { }
    }
}
