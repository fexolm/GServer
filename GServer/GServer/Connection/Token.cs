using System;
using System.IO;
using System.Text;
using GServer.Containers;

namespace GServer.Connection
{
    public class Token : ISerializable, IComparable, IDeepSerializable, IDeepDeserializable
    {
        private string _tokenStr;

        public Token() { }

        public Token(string str) {
            _tokenStr = str;
        }

        public static Token GenerateToken() {
            return new Token(GetRandomString(32));
        }

        public byte[] Serialize() {
            using (var m = new MemoryStream()) {
                using (var writer = new BinaryWriter(m)) {
                    writer.Write(_tokenStr);
                }
                return m.ToArray();
            }
        }

        public override string ToString() {
            return _tokenStr;
        }

        public override bool Equals(object obj) {
            try {
                var left = (Token) obj;
                return left != null && left._tokenStr == _tokenStr;
            }
            catch {
                return false;
            }
        }

        public static bool operator ==(Token left, Token right) {
            return Equals(left, right);
        }

        public static bool operator !=(Token left, Token right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _tokenStr.GetHashCode();
        }

        public int CompareTo(object obj) {
            var other = (Token) obj;
            return string.Compare(_tokenStr, other._tokenStr, StringComparison.Ordinal);
        }

        public void PushToDs(DataStorage ds) {
            ds.Push(_tokenStr);
        }

        public void ReadFromDs(DataStorage ds) {
            _tokenStr = ds.ReadString();
        }

        private static readonly Random random = new Random((int) DateTime.Now.Ticks);

        private static string GetRandomString(int size) {
            var builder = new StringBuilder();
            for (var i = 0; i < size; i++) {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}