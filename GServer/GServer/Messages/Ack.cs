using System;
using System.Collections.Generic;
using System.Linq;
namespace GServer
{
    internal class Ack
    {
        private MessageCounter _lastMessageNum = 0;
        public Ack()
        {
            _pendingMessages = new Dictionary<MessageCounter, Message>();
            _notYetArrivedMessages = new List<MessageCounter>();
        }
        private IDictionary<MessageCounter, Message> _pendingMessages;
        private MessageCounter _lastRecievedMessage;
        private IList<MessageCounter> _notYetArrivedMessages;
        private Queue<MessageCounter> _resendInterval = new Queue<MessageCounter>();
        public void StoreReliable(Message msg)
        {
            lock (_pendingMessages)
            {
                if (!_pendingMessages.ContainsKey(msg.MessageId))
                {
                    _pendingMessages.Add(msg.MessageId, msg);
                    _resendInterval.Enqueue(msg.MessageId);
                    if (_resendInterval.Count > 5)
                        _resendInterval.Dequeue();
                }
            }
        }
        public int ReceiveReliable(Message msg)
        {
            int bitField = 0;
            lock (_notYetArrivedMessages)
            {
                MessageCounter counter;
                if (msg.MessageId > _lastRecievedMessage)
                {
                    counter = _lastRecievedMessage;
                    counter++;
                    while (counter != msg.MessageId)
                    {
                        _notYetArrivedMessages.Add(counter);
                        counter++;
                    }
                    _lastRecievedMessage = msg.MessageId;
                }
                else
                {
                    if (_notYetArrivedMessages.Contains(msg.MessageId))
                    {
                        _notYetArrivedMessages.Remove(msg.MessageId);
                    }
                }
                counter = msg.MessageId;
                for (int i = 0; i < 32; i++)
                {
                    if (!_notYetArrivedMessages.Contains(counter))
                    {
                        bitField |= 1 << i;
                    }
                    counter--;
                }
            }
            return bitField;
        }
        public void ProcessReceivedAckBitfield(int bitField, MessageCounter msgId)
        {
            IEnumerable<KeyValuePair<MessageCounter, Message>> toRemove = null;

            lock (_pendingMessages)
            {
                var tmp = msgId;
                while (bitField != 0)
                {
                    if ((bitField & 1) == 1)
                    {
                        if (_pendingMessages.ContainsKey(tmp))
                        {
                            _pendingMessages.Remove(tmp);
                        }
                    }
                    bitField <<= 1;
                    tmp--;
                }
                toRemove = _pendingMessages.Where(x => msgId - x.Key > 30 && !_resendInterval.Contains(x.Key)).ToArray();
                foreach (var element in toRemove)
                {
                    _pendingMessages.Remove(element);
                }
            }
            foreach (var element in toRemove)
            {
                PacketLost?.Invoke(element.Value);
            }
        }
        public event Action<Message> PacketLost;
    }
}

