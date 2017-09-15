using System;
using System.Collections.Generic;
using GServer.Containers;

namespace GServer.Messages
{
    internal class Ack
    {
        public Ack() {
            _ackBuffer = new List<MessageCounter>();
        }

        private readonly List<MessageCounter> _ackBuffer;

        public void ReceiveReliable(Message msg) {
            lock (_ackBuffer) {
                if (!_ackBuffer.Contains(msg.MessageId)) {
                    _ackBuffer.Add(msg.MessageId);
                }
            }
        }

        public void ProcessReceivedAckBitfield(int bitField, MessageCounter msgId, short msgType) {
            var tmp = msgId;
            var bf = (uint) bitField;
            while (bf != 0) {
                if ((bf & 1) == 1) {
                    // ReSharper disable once UseNullPropagation
                    if (MessageArrived != null) MessageArrived.Invoke(tmp, msgType);
                }
                bf >>= 1;
                tmp--;
            }
        }

        public event Action<MessageCounter, short> MessageArrived;
        public static readonly IEnumerable<Pair<MessageCounter, int>> Empty = new List<Pair<MessageCounter, int>>();

        public IEnumerable<Pair<MessageCounter, int>> GetAcks() {
            lock (_ackBuffer) {
                if (_ackBuffer.Count == 0) {
                    return Empty;
                }
                var res = new List<Pair<MessageCounter, int>>();
                _ackBuffer.Sort((self, other) => other.CompareTo(self));
                var i = 1;
                var len = _ackBuffer.Count;
                var shift = 0;
                res.Add(new Pair<MessageCounter, int>(_ackBuffer[0], 1));
                for (; i < len; i++) {
                    var dif = Math.Abs(_ackBuffer[i] - _ackBuffer[i - 1]);
                    if (dif == 0) {
                        len--;
                        i--;
                        continue;
                    }
                    if (shift + dif < 32) {
                        shift += dif;
                        res[res.Count - 1].Val2 |= (1 << shift);
                    }
                    else {
                        res.Add(new Pair<MessageCounter, int>(_ackBuffer[i], 1));
                        shift = 0;
                    }
                }
                _ackBuffer.Clear();
                return res;
            }
        }
    }
}