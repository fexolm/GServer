using GServer.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GServer
{
    public class MessageQueue : IEnumerable<KeyValuePair<short, Message>>
    {
        private SortedList<short, Message> _msgQueue;
        public MessageQueue()
        {
            _msgQueue = new SortedList<short, Message>();
        }
        public void Add(Message msg)
        {
            _msgQueue.Add(msg.Header.MessageId, msg);
        }
        public IEnumerator<KeyValuePair<short, Message>> GetEnumerator()
        {
            return _msgQueue.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _msgQueue.GetEnumerator();
        }
        public void Remove(Message msg)
        {
            _msgQueue.Remove(msg.Header.MessageId);
        }
    }
    public class Connection
    {
        public readonly IPEndPoint EndPoint;
        public readonly Token Token;
        public DateTime LastActivity { get; private set; }
        public Connection(IPEndPoint endPoint)
            : this(endPoint, Token.GenerateToken())
        {
        }
        public Connection(IPEndPoint endPoint, Token token)
        {
            EndPoint = endPoint;
            Token = token;
            LastActivity = DateTime.Now;
            _AckPerMsgType = new Dictionary<short, Ack>();
            _lastSequencedMessageNumPerType = new Dictionary<short, short>();
            _lastOrderedMessageNumPerType = new Dictionary<short, short>();
            _messageQueuePerType = new SortedDictionary<short, MessageQueue>();
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

        #region Reliable

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

        #endregion

        #region Sequenced

        private readonly IDictionary<short, short> _lastSequencedMessageNumPerType;
        public bool IsMessageInItsOrder(short type, short num)
        {
            lock (_lastSequencedMessageNumPerType)
            {
                if (_lastSequencedMessageNumPerType.ContainsKey(type))
                {
                    if (_lastSequencedMessageNumPerType[type] < num)
                    {
                        _lastSequencedMessageNumPerType[type] = num;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _lastSequencedMessageNumPerType.Add(type, num);
                    return true;
                }
            }
        }
        #endregion

        #region Ordered

        private IDictionary<short, short> _lastOrderedMessageNumPerType;
        private SortedDictionary<short, MessageQueue> _messageQueuePerType;
        public List<Message> MessagesToInvoke(Message msg)
        {
            List<Message> messagesToInvoke = new List<Message>();

            lock (_lastOrderedMessageNumPerType)
            {
                if (!_lastOrderedMessageNumPerType.ContainsKey((short)msg.Header.Type))
                {
                    _lastOrderedMessageNumPerType.Add((short)msg.Header.Type, 0);
                    lock (_messageQueuePerType)
                    {
                        _messageQueuePerType.Add((short)msg.Header.Type, new MessageQueue());
                    }
                }
                lock (_messageQueuePerType)
                {
                    var currentTypeQueue = _messageQueuePerType[(short)msg.Header.Type];
                    currentTypeQueue.Add(msg);
                    foreach (var element in currentTypeQueue)
                    {
                        if (element.Key == _lastOrderedMessageNumPerType[(short)msg.Header.Type] + 1)
                        {
                            messagesToInvoke.Add(element.Value);
                            _lastOrderedMessageNumPerType[(short)msg.Header.Type]++;
                        }
                    }
                    foreach (var element in messagesToInvoke)
                    {
                        currentTypeQueue.Remove(element);
                    }
                }
                return messagesToInvoke;
            }
        #endregion
        }
    }
}
