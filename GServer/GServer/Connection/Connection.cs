using GServer.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GServer
{
    public class Connection
    {
        public readonly IPEndPoint EndPoint;
        public readonly Token Token;
        public DateTime LastActivity { get; private set; }
        public Connection(IPEndPoint endPoint) :this(endPoint, Token.GenerateToken())
        { 
        }
        public Connection(IPEndPoint endPoint, Token token)
        {
            EndPoint = endPoint;
            Token = token;
            LastActivity = DateTime.Now;
            _AckPerMsgType = new Dictionary<short, Ack>();
        }
        public void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }
        public event Action<Connection> Disconnected;
        public void Disconnect()
        {
            if (Disconnected != null)
                Disconnected.Invoke(this);
        }

        private readonly IDictionary<short, Ack> _AckPerMsgType;
        public Message GenerateAck(Message msg)
        {
            int bitField;
            lock (_AckPerMsgType)
            {
                if (_AckPerMsgType.ContainsKey((short)msg.Header.Type))
                {
                    bitField = _AckPerMsgType[(short)msg.Header.Type].GetStatistic(msg.Header.MessageId);
                }
                else
                {
                    _AckPerMsgType.Add((short)msg.Header.Type, new Ack(msg.Header.MessageId));
                    bitField = 1;
                }
            }
            return Message.Ack(msg.Header, bitField);
        }
    }
}
