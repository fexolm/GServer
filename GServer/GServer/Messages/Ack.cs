﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer.Messages
{
    public class Ack
    {
        private short _lastMessageNum;
        private int _lastMessagesStat;
        public Ack(int _lastMessagesStat)
        {
            _lastMessageNum = 0;
            _lastMessagesStat = 1;
        }
        public int GetStatistic(short curNum)
        {
            int diff = curNum - _lastMessageNum;
            if (diff > 0)
            {
                if (diff >= 32)
                {
                    _lastMessagesStat = 1;
                }
                else
                {
                    _lastMessagesStat = (_lastMessagesStat << diff) | 1;
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
    }
    public class AckManager
    {

    }
}