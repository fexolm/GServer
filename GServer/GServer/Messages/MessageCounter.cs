using System;

namespace GServer
{
    internal struct MessageCounter : IComparable
    {
        private short _count;
        public static readonly MessageCounter Default = new MessageCounter(short.MinValue);
        public MessageCounter(short value)
        {
            _count = value;
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
        public static bool operator <=(MessageCounter left, MessageCounter right)
        {
            return left.CompareTo(right) < 0 || left.CompareTo(right) == 0;
        }
        public static bool operator >=(MessageCounter left, MessageCounter right)
        {
            return left.CompareTo(right) > 0 || left.CompareTo(right) == 0;
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

        public static MessageCounter operator --(MessageCounter mc)
        {
            if (mc._count > short.MinValue)
            {
                mc._count--;
            }
            else
            {
                mc._count = short.MaxValue;
            }
            return mc;
        }
        public static int operator -(MessageCounter left, MessageCounter right)
        {
            var dif = left._count - right._count;
            if (Math.Abs(dif) > (1 + short.MaxValue))
            {
                return Math.Abs(dif) - short.MaxValue - 1;
            }
            else
            {
                return dif;
            }
        }
        public static int operator +(MessageCounter left, MessageCounter right)
        {
            var dif = left._count + right._count;
            if (Math.Abs(dif) > (1 + short.MaxValue))
            {
                return Math.Abs(dif) - short.MaxValue - 1;
            }
            else
            {
                return dif;
            }
        }
        public static int operator -(MessageCounter left, short right)
        {
            var dif = left._count - right;
            if (Math.Abs(dif) > (1 + short.MaxValue))
            {
                return Math.Abs(dif) - short.MaxValue - 1;
            }
            else
            {
                return dif;
            }
        }
        public static int operator -(MessageCounter left, int right)
        {
            var dif = left._count - right;
            if (Math.Abs(dif) > (1 + short.MaxValue))
            {
                return Math.Abs(dif) - short.MaxValue - 1;
            }
            else
            {
                return dif;
            }
        }
        public short ToShort()
        {
            return _count;
        }
        public static implicit operator MessageCounter(short val)
        {
            return new MessageCounter(val);
        }
        public static implicit operator MessageCounter(int val)
        {
            return new MessageCounter((short)val);
        }
        public static explicit operator short(MessageCounter val)
        {
            return val._count;
        }
        public override string ToString()
        {
            return _count.ToString();
        }
    }
}
