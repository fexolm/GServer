using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    public enum MessageType : byte
    {
        Handshake = 0,
    }

    public class Message : ISerializable
    {
        public MessageType Type{get;private set;}
        public static readonly Message Handshake = new Message { Type = MessageType.Handshake };

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
