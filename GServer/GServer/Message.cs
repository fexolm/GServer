using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    public enum MessageType : byte
    {
        Handshake = 0,
        Ping = 1,
    }

    public class Header : ISerializable
    {
        public Token ConnectionToken { get; private set; }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
        public static Header Deserialize(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
    public class Message : ISerializable
    {
        public MessageType Type{get;private set;}
        public Header Header { get; private set; }
        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
        public static Message Deserialize(byte[] buffer)
        {
            throw new NotImplementedException();
        }
        public static readonly Message Handshake = new Message { Type = MessageType.Handshake };
    }
}
