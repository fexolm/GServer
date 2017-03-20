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
        private List<MessageCounter> _notYetArrivedMessages;
        private Queue<MessageCounter> _last30 = new Queue<MessageCounter>();
        public void StoreReliable(Message msg)
        {
            lock (_pendingMessages)
            {
                if (!_pendingMessages.ContainsKey(msg.MessageId))
                {
                    _pendingMessages.Add(msg.MessageId, msg);
                    _last30.Enqueue(msg.MessageId);
                    if (_last30.Count > 30)
                        _last30.Dequeue();
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
                while (bitField != 0)
                {
                    if ((bitField & 1) == 1)
                    {
                        if (_pendingMessages.ContainsKey(msgId))
                        {
                            _pendingMessages.Remove(msgId);
                            Console.WriteLine("Пришло потеряное сообщение {0}", msgId);
                        }
                    }
                    bitField <<= 1;
                    msgId--;
                }
                toRemove = _pendingMessages.Where(x => msgId - x.Key > 30 && !_last30.Contains(x.Key)).ToArray();
                foreach (var element in toRemove)
                {
                    _pendingMessages.Remove(element);
                }
            }
            foreach (var element in toRemove)
            {
                PacketLost?.Invoke(element.Value);
                Console.WriteLine("---------------Потерялось сообщение {0}", element.Value.MessageId);
            }
        }
        public event Action<Message> PacketLost;
    }
}

