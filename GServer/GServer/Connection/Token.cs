using System;
using System.IO;
using GServer.Containers;
using System.Text;

namespace GServer
{
	public class Token : ISerializable, IComparable, IDeepSerializable, IDeepDeserializable
	{
		private string _tokenStr;

		public Token()
		{
		}
		public Token(string str)
		{
			_tokenStr = str;
		}
		public static Token GenerateToken()
		{
			return new Token(GetRandomString(32));
		}
		public byte[] Serialize()
		{
			using (MemoryStream m = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(m))
				{
					writer.Write(_tokenStr);
				}
				return m.ToArray();
			}
		}
		public override string ToString()
		{
			return _tokenStr;
		}
		public override bool Equals(object obj)
		{
			try
			{
				var left = (Token)obj;
				return left._tokenStr == _tokenStr;
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
			return _tokenStr.GetHashCode();
		}

		public int CompareTo(object obj)
		{
			var other = (Token)obj;
			return this._tokenStr.CompareTo(other._tokenStr);
		}

		public void PushToDs(DataStorage ds)
		{
			ds.Push(_tokenStr);
		}

		public void ReadFromDs(DataStorage ds)
		{
			_tokenStr = ds.ReadString();
		}

		private static Random random = new Random((int)DateTime.Now.Ticks);

		private static string GetRandomString(int size)
		{
			StringBuilder builder = new StringBuilder();
			char ch;
			for (int i = 0; i < size; i++)
			{
				ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
				builder.Append(ch);
			}

			return builder.ToString();
		}
	}
}
