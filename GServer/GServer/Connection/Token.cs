using System;
using System.IO;

namespace GServer
{
    public class Token : ISerializable, IComparable
    {
        private static int _globalNum = 0;

        private readonly int _tempNum;

        private Token()
        {
            _tempNum = _globalNum;
            _globalNum++;
        }
        public Token(int num)
        {
            _tempNum = num;
        }
        public static Token GenerateToken()
        {
            return new Token();
        }
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(_tempNum);
                }
                return m.ToArray();
            }
        }
        public override string ToString()
        {
            return _tempNum.ToString();
        }
        public override bool Equals(object obj)
        {
            try
            {
                var left = (Token)obj;
                return left._tempNum == _tempNum;
            }
            catch
            {
                return false;
            }
        }
        public static bool operator ==(Token left, Token right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Token left, Token right)
        {
            return !Equals(left, right);
        }
        public override int GetHashCode()
        {
            return _tempNum.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            var other = (Token)obj;
            return this._tempNum.CompareTo(other._tempNum);
        }
    }
}
