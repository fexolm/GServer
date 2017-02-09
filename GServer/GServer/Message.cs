using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    public enum MessageType : byte
    {
        Empty = 0,
        Handshake,
        Ping,
        Rpc,
        Authorization,
    }

    public class Header : ISerializable
    {
        public MessageType Type { get; private set; }
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
        public Header Header { get; private set; }
        public byte[] body { get; private set; }
        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
        public static Message Deserialize(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
