using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    public class FlagContainer
    {
        private int _flagCount = 0;
        private byte _buffer = 0;
        public FlagContainer(params bool[] flags)
        {
            foreach (var flag in flags)
            {
                Push(flag);
            }
        }
        public FlagContainer(byte buffer)
        {
            _buffer = buffer;
        }
        public void Push(bool flag)
        {
            if (_flagCount != 0)
            {
                _buffer = (byte)(_buffer << 1);
            }
            _buffer += (byte)(flag ? 1 : 0);
            _flagCount++;
        }
        public bool Pop()
        {
            bool res = _buffer % 2 == 1;
            _buffer = (byte)(_buffer >> 1);
            _flagCount--;
            return res;
        }
        public int Length { get { return _flagCount; } }
        public byte GetByte()
        {
            return _buffer;
        }
    }
}
