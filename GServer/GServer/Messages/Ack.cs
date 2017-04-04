using System;
using System.Collections.Generic;
namespace GServer
{
    internal class Ack
    {
        private MessageCounter _lastMessageNum = 0;
        public Ack()
        {
            _notYetArrivedMessages = new List<MessageCounter>();
        }
        private MessageCounter _lastRecievedMessage;
        private IList<MessageCounter> _notYetArrivedMessages;
        private Queue<MessageCounter> _resendInterval = new Queue<MessageCounter>();
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
        public void ProcessReceivedAckBitfield(int bitField, MessageCounter msgId, short msgType)
        {
            var tmp = msgId;
            while (bitField != 0)
            {
                if ((bitField & 1) == 1)
                {
                    MessageArrived?.Invoke(msgId, msgType);
                }
                bitField <<= 1;
                tmp--;
            }
        }
        public event Action<MessageCounter, short> MessageArrived;
    }
}

