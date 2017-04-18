using GServer.Containers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace GServer
{
    internal class Packet : ISerializable
    {
        public int Priority { get; set; }
        public Message Msg { get; private set; }
        public bool Resend => Msg.Reliable && !Msg.Sequenced;
        public byte[] Serialize()
        {
            var buffer = Msg.Serialize();
            return new DataStorage().Push(buffer.Length).Push(buffer).Serialize();
        }
        public Packet(Message msg)
        {
            Msg = msg;
            Priority = 0;
        }
    }

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
            _lastUnOrderedMessageNumPerType = new Dictionary<short, MessageCounter>();
            _messageQueuePerType = new SortedDictionary<short, MessageQueue>();
            _arrivedReliableMessagePerType = new Dictionary<short, Pair<CustomList<MessageCounter>, MessageCounter>>();
        }
        public event Action Disconnected;

        public int BufferCount
        {
            get
            {
                lock (_messageBuffer)
                {
                    return _messageBuffer.Count;
                }
            }
        }

        private List<Packet> _messageBuffer = new List<Packet>();
        internal void MarkToSend(Message msg)
        {
            var p = new Packet(msg);
            lock (_messageBuffer)
            {
                _messageBuffer.Add(p);
            }
        }
        internal byte[] GetBytesToSend()
        {
            int i = 0;
            List<Packet> toSend = new List<Packet>();
            foreach (var ack in _ackPerMsgType)
            {
                var buffer = ack.Value.GetAcks();
                if (buffer != Ack.Empty)
                {
                    foreach (var msg in buffer)
                    {
                        i++;
                        toSend.Add(new Packet(Message.Ack(ack.Key, msg.Val1, Token, msg.Val2)));
                    }
                }
            }
            lock (_messageBuffer)
            {
                List<Packet> toDelete = new List<Packet>();
                _messageBuffer.Sort((x, y) =>
                {
                    int p = x.Priority.CompareTo(y.Priority);
                    return p == 0 ? x.Msg.MessageId.CompareTo(y.Msg.MessageId) : -p;
                });
                for (; i < 128 && i < _messageBuffer.Count; i++)
                {
                    toSend.Add(_messageBuffer[i]);
                    if (!_messageBuffer[i].Resend)
                    {
                        toDelete.Add(_messageBuffer[i]);
                    }
                }
                for (; i < _messageBuffer.Count; i++)
                {
                    _messageBuffer[i].Priority++;
                }
                foreach (var element in toDelete)
                {
                    _messageBuffer.Remove(element);
                }
            }

            DataStorage ds = new DataStorage();
            foreach (var element in toSend)
            {
                ds.Push(element.Serialize());
            }
            return ds.Serialize();
        }
        internal void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }
        internal void Disconnect()
        {
            if (Disconnected != null)
                Disconnected.Invoke();
        }
        #region Reliable
        private readonly IDictionary<short, Ack> _ackPerMsgType;
        private readonly IDictionary<short, Pair<CustomList<MessageCounter>, MessageCounter>> _arrivedReliableMessagePerType;
        internal void ReceiveReliable(Message msg)
        {
            lock (_ackPerMsgType)
            {
                if (_ackPerMsgType.ContainsKey(msg.Header.Type))
                {
                    _ackPerMsgType[msg.Header.Type].ReceiveReliable(msg);
                }
                else
                {
                    var ack = new Ack();
                    _ackPerMsgType.Add(msg.Header.Type, ack);
                    ack.MessageArrived += AckArrivedHandler;
                    ack.ReceiveReliable(msg);
                }
            }
        }
        internal bool HasAlreadyArrived(Message msg)
        {
            var msgId = msg.MessageId;
            lock (_arrivedReliableMessagePerType)
            {
                if (!_arrivedReliableMessagePerType.ContainsKey(msg.Header.Type))
                {
                    var list = new CustomList<MessageCounter>();
                    _arrivedReliableMessagePerType.Add(msg.Header.Type, new Pair<CustomList<MessageCounter>, MessageCounter>(list, -1));
                }
                var pair = _arrivedReliableMessagePerType[msg.Header.Type];
                var arrivedMessages = pair.Val1;

                var node = arrivedMessages.First;
                CustomNode<MessageCounter> res = null;
                int pos = 0;
                if (msgId >= pair.Val2)
                {
                    if (msgId == pair.Val2)
                    {
                        return true;
                    }
                    if (msgId > pair.Val2 + 1)
                    {
                        arrivedMessages.PushBack(pair.Val2);
                        arrivedMessages.PushBack(msgId);
                        _arrivedReliableMessagePerType[msg.Header.Type].Val2 = msgId;
                        return false;
                    }
                    _arrivedReliableMessagePerType[msg.Header.Type].Val2 = msgId;
                    return false;
                }
                if (arrivedMessages.Empty)
                {
                    return true;
                }
                if (msgId < arrivedMessages.First.Value)
                {
                    return true;
                }
                for (int i = 0, count = arrivedMessages.Count; i < count && node.Value <= msgId; i++)
                {
                    if (node.Value == msgId)
                    {
                        return true;
                    }
                    if (node.Value < msgId)
                    {
                        res = node;
                        pos = i;
                    }
                    node = node.Next;
                }

                if ((pos & 1) == 1)
                {
                    return true;
                }
                else
                {
                    if (res.Value == msgId - 1)
                    {
                        if (res.Next.Value == msgId + 1)
                        {
                            arrivedMessages.RemoveBetween(res.Prev, res.Next.Next);
                        }
                        else
                        {
                            res.Value = msgId;
                        }
                    }
                    else if (res.Next.Value == msgId + 1)
                    {
                        res.Next.Value = msgId;
                    }
                    else
                    {
                        arrivedMessages.InsertAfter(res, msgId);
                        arrivedMessages.InsertAfter(res.Next, msgId);
                    }
                }
                return false;
            }
        }
        private void AckArrivedHandler(MessageCounter arg1, short arg2)
        {
            lock (_messageBuffer)
            {
                var toRemove = _messageBuffer.FirstOrDefault(m =>
                                                m.Msg.MessageId == arg1
                                                && m.Msg.Header.Type == arg2);
                if (toRemove != null)
                {
                    _messageBuffer.Remove(toRemove);
                    Console.WriteLine("Removing {0}", toRemove.Msg.MessageId);
                }
            }
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
            ack?.ProcessReceivedAckBitfield(bitField, msg.MessageId, msgType);
        }
        internal void StoreReliable(Message msg)
        {
            Ack ack;
            lock (_ackPerMsgType)
            {
                if (!_ackPerMsgType.ContainsKey(msg.Header.Type))
                {
                    ack = new Ack();
                    _ackPerMsgType.Add((short)msg.Header.Type, ack);
                    ack.MessageArrived += AckArrivedHandler;
                }
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
                    if (!_messageQueuePerType.ContainsKey(msg.Header.Type))
                    {
                        _messageQueuePerType.Add(msg.Header.Type, new MessageQueue());
                    }
                    if (msg.MessageId < _lastOrderedMessageNumPerType[msg.Header.Type])
                    {
                        return messagesToInvoke;
                    }
                    var currentTypeQueue = _messageQueuePerType[msg.Header.Type];
                    currentTypeQueue.Add(msg);
                    foreach (var element in currentTypeQueue)
                    {
                        if (element.Key == _lastOrderedMessageNumPerType[msg.Header.Type])
                        {
                            messagesToInvoke.Add(element.Value);
                            _lastOrderedMessageNumPerType[msg.Header.Type]++;
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
        #region Unordered
        private IDictionary<short, MessageCounter> _lastUnOrderedMessageNumPerType;
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
            else
            {
                lock (_lastUnOrderedMessageNumPerType)
                {
                    if (!_lastUnOrderedMessageNumPerType.ContainsKey((short)msg.Header.Type))
                    {
                        _lastUnOrderedMessageNumPerType.Add((short)msg.Header.Type, 0);
                    }
                    result = _lastUnOrderedMessageNumPerType[(short)msg.Header.Type];
                    _lastUnOrderedMessageNumPerType[(short)msg.Header.Type]++;
                }
            }
            return result;
        }
    }
}
