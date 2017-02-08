using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GServer
{
    public class Token : ISerializable
    {
        private string TokenStr;
        public Token(string str)
        {
            TokenStr = str;
        }
        public static Token GenerateToken()
        {
            return new Token(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
        }
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(TokenStr);
                }
                return m.ToArray();
            }
        }
        public override string ToString()
        {
            return TokenStr;
        }
        public override bool Equals(object obj)
        {
            try
            {
                var left = (Token)obj;
                return left.TokenStr == TokenStr;
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
            return TokenStr.GetHashCode();
        }
    }
}
