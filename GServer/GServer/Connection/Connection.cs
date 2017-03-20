using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace GServer
{
    internal class MessageQueue : IEnumerable<KeyValuePair<MessageCounter, Message>>
    {
        private SortedList<MessageCounter, Message> _msgQueue;
        public MessageQueue()
        {
            _msgQueue = new SortedList<MessageCounter, Message>();
        }
        public void Add(Message msg)
        {
            if (!_msgQueue.ContainsKey(msg.MessageId))
                _msgQueue.Add(msg.Header.MessageId, msg);
        }
        public IEnumerator<KeyValuePair<MessageCounter, Message>> GetEnumerator()
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
            _ackPerMsgType = new Dictionary<short, Ack>();
            _lastSequencedMessageNumPerType = new Dictionary<short, MessageCounter>();
            _lastOrderedMessageNumPerType = new Dictionary<short, MessageCounter>();
            _messageQueuePerType = new SortedDictionary<short, MessageQueue>();
        }
        internal void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }
        public event Action<Connection> Disconnected;
        internal void Disconnect()
        {
            if (Disconnected != null)
                Disconnected.Invoke(this);
        }

        #region Reliable

        private readonly IDictionary<short, Ack> _ackPerMsgType;
        internal Message GenerateAck(Message msg)
        {
            int bitField;
            lock (_ackPerMsgType)
            {
                if (_ackPerMsgType.ContainsKey(msg.Header.Type))
                {
                    bitField = _ackPerMsgType[msg.Header.Type].ReceiveReliable(msg);
                }
                else
                {
                    var ack = new Ack();
                    _ackPerMsgType.Add((short)msg.Header.Type, ack);
                    bitField = 1;
                }
            }
            return Message.Ack(msg.Header, bitField);
        }
        internal void ProcessAck(Message msg)
        {
            Ack ack = null;
            var ds = new DataStorage(msg.Body);
            int bitField = ds.ReadInt32();
            short msgType = ds.ReadInt16();
            lock (_ackPerMsgType)
            {
                if (_ackPerMsgType.ContainsKey(msgType))
                {
                    ack = _ackPerMsgType[msgType];
                }
            }
            ack?.ProcessReceivedAckBitfield(bitField, msg.MessageId);
        }
        internal void StoreReliable(Message msg)
        {
            Ack ack;
            lock (_ackPerMsgType)
            {
                if (_ackPerMsgType.ContainsKey(msg.Header.Type))
                {
                    ack = _ackPerMsgType[msg.Header.Type];
                }
                else
                {
                    ack = new Ack();
                    _ackPerMsgType.Add((short)msg.Header.Type, ack);
                    ack.PacketLost += PacketLostHander;
                }
            }
            ack.StoreReliable(msg);
        }
        private void PacketLostHander(Message obj)
        {
            if (obj.Header.Ordered)
            {
                OrderedLost?.Invoke(this, obj);
                return;
            }
            if (obj.Header.Sequenced)
            {
                SequencedLost?.Invoke(this, obj);
                return;
            }
        }
        #endregion
        #region Sequenced

        private readonly IDictionary<short, MessageCounter> _lastSequencedMessageNumPerType;
        internal bool IsMessageInItsOrder(short type, MessageCounter num)
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

        private IDictionary<short, MessageCounter> _lastOrderedMessageNumPerType;
        private SortedDictionary<short, MessageQueue> _messageQueuePerType;
        internal List<Message> MessagesToInvoke(Message msg)
        {
            List<Message> messagesToInvoke = new List<Message>();

            lock (_lastOrderedMessageNumPerType)
            {
                lock (_messageQueuePerType)
                {
                    if (!_lastOrderedMessageNumPerType.ContainsKey(msg.Header.Type))
                    {
                        _lastOrderedMessageNumPerType.Add(msg.Header.Type, 0);
                    }
                    if (!_messageQueuePerType.ContainsKey((short)msg.Header.Type))
                    {
                        _messageQueuePerType.Add(msg.Header.Type, new MessageQueue());
                    }
                    var currentTypeQueue = _messageQueuePerType[(short)msg.Header.Type];
                    currentTypeQueue.Add(msg);
                    foreach (var element in currentTypeQueue)
                    {
                        if (element.Key == _lastOrderedMessageNumPerType[(short)msg.Header.Type])
                        {
                            messagesToInvoke.Add(element.Value);
                            _lastOrderedMessageNumPerType[(short)msg.Header.Type]++;
                        }
                    }
                    foreach (var element in messagesToInvoke)
                    {
                        currentTypeQueue.Remove(element);
                    }
                    return messagesToInvoke;
                }
            }
        }
        #endregion
        internal MessageCounter GetMessageId(Message msg)
        {
            MessageCounter result = MessageCounter.Default;
            if (msg.Ordered)
            {
                lock (_lastOrderedMessageNumPerType)
                {
                    if (!_lastOrderedMessageNumPerType.ContainsKey((short)msg.Header.Type))
                    {
                        _lastOrderedMessageNumPerType.Add((short)msg.Header.Type, 0);
                    }
                    result = _lastOrderedMessageNumPerType[(short)msg.Header.Type];
                    _lastOrderedMessageNumPerType[(short)msg.Header.Type]++;
                }
            }
            else if (msg.Sequenced)
            {
                lock (_lastSequencedMessageNumPerType)
                {
                    if (!_lastSequencedMessageNumPerType.ContainsKey((short)msg.Header.Type))
                    {
                        _lastSequencedMessageNumPerType.Add((short)msg.Header.Type, 0);
                    }
                    result = _lastSequencedMessageNumPerType[(short)msg.Header.Type];
                    _lastSequencedMessageNumPerType[(short)msg.Header.Type]++;
                }
            }
            return result;
        }
        internal static Action<Connection, Message> OrderedLost;
        internal static Action<Connection, Message> SequencedLost;
    }
}
