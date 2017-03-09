using System;
using System.Collections.Generic;

namespace GServer
{
    internal class Ack
    {
        private MessageCounter _lastMessageNum;
        private int _lastMessagesStat;
        public Ack()
        {
            _lastMessageNum = 0;
            _lastMessagesStat = -1;
        }
        public int GetStatistic(MessageCounter curNum)
        {
            int diff = curNum - _lastMessageNum;
            if (diff > 0)
            {
                int lostPackets;
                if (diff >= 32)
                {
                    lostPackets = ~_lastMessagesStat;
                    _lastMessagesStat = 1;
                    List<MessageCounter> lostMessages = new List<MessageCounter>();
                    for (int i = 0; i < diff; i++)
                    {
                        bool isLost = (lostPackets & 1) == 1;
                        lostPackets = lostPackets << 1;
                        i++;
                        if (isLost)
                        {
                            short lostPacketNumber = (short)((short)curNum - diff - (32 - i));
                            lostMessages.Add(lostPacketNumber);
                        }
                    }
                    for (int i = (short)curNum - diff; i < (short)curNum - 32; i++)
                    {
                        lostMessages.Add((short)i);
                    }
                    PacketLost?.Invoke(lostMessages);
                }
                else
                {
                    lostPackets = ~(_lastMessagesStat >> (32 - diff));
                    _lastMessagesStat = (_lastMessagesStat << diff) | 1;
                    if (lostPackets != 0)
                    {
                        List<MessageCounter> lostMessages = new List<MessageCounter>();

                        for (int i = 0; i < diff; i++)
                        {
                            bool isLost = (lostPackets & 1) == 1;
                            lostPackets = lostPackets << 1;
                            i++;
                            if (isLost)
                            {
                                short lostPacketNumber = (short)((short)curNum - diff - (32 - i));
                                lostMessages.Add(lostPacketNumber);
                            }
                        }
                        PacketLost?.Invoke(lostMessages);
                    }
                }
                _lastMessageNum = curNum;
            }
            else
            {
                if (diff > -32)
                {
                    _lastMessagesStat = _lastMessagesStat | (1 << (-diff));
                }
            }
            return _lastMessagesStat;
        }
        public event Action<List<MessageCounter>> PacketLost;
    }
}
