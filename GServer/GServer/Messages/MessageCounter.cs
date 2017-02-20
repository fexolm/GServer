using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GServer
{
    public class MessageCounter : IComparable
    {
        private short _count;
        public MessageCounter(short value)
        {
            _count = value;
        }
        public MessageCounter()
        {
            _count = short.MinValue;
        }
        public int CompareTo(object obj)
        {
            var other = (MessageCounter)obj;
            return _count.CompareTo(other._count) * (Math.Abs(this._count - other._count) < (short.MaxValue / 2) ? 1 : -1);
        }
        public static bool operator ==(MessageCounter left, MessageCounter right)
        {
            return left.CompareTo(right) == 0;
        }
        public static bool operator !=(MessageCounter left, MessageCounter right)
        {
            return left.CompareTo(right) != 0;
        }
        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator <(MessageCounter left, MessageCounter right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator >(MessageCounter left, MessageCounter right)
        {
            return left.CompareTo(right) > 0;
        }
        public static MessageCounter operator ++(MessageCounter mc)
        {
            if (mc._count < short.MaxValue)
            {
                mc._count++;
            }
            else
            {
                mc._count = short.MinValue;
            }
            return mc;
        }
        public short ToShort()
        {
            return _count;
        }
        public static explicit operator MessageCounter(short val)
        {
            return new MessageCounter(val);
        }
        public static explicit operator short(MessageCounter val)
        {
            return val._count;
        }
    }
}
