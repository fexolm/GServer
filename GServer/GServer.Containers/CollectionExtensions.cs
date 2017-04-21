using System;
using System.Collections.Generic;
using System.Linq;
namespace GServer.Containers
{
    public static class CollectionExtensions
    {
        public static void Invoke<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach(var element in collection)
            {
                action.Invoke(element);
            }
        }

        #region IEnumerable Serialization
        public static byte[] Serialize(this IEnumerable<int> collection)
        {
            var ds = new DataStorage();
            ds.Push(collection.Count());
            foreach(var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static IEnumerable<int> FillDerialize(this IEnumerable<int> collection, byte[] buffer)
        {
            var ds = new DataStorage(buffer);
            int len = ds.ReadInt32();
            List<int> res = new List<int>();
            for(int i=0; i<len; i++)
            {
                res.Add(ds.ReadInt32());
            }
            return res;
        }
        public static byte[] Serialize(this IEnumerable<long> collection)
        {
            var ds = new DataStorage();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static IEnumerable<long> FillDerialize(this IEnumerable<long> collection, byte[] buffer)
        {
            var ds = new DataStorage(buffer);
            int len = ds.ReadInt32();
            List<long> res = new List<long>();
            for (int i = 0; i < len; i++)
            {
                res.Add(ds.ReadInt64());
            }
            return res;
        }
        public static byte[] Serialize(this IEnumerable<short> collection)
        {
            var ds = new DataStorage();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static IEnumerable<short> FillDerialize(this IEnumerable<short> collection, byte[] buffer)
        {
            var ds = new DataStorage(buffer);
            int len = ds.ReadInt32();
            List<short> res = new List<short>();
            for (int i = 0; i < len; i++)
            {
                res.Add(ds.ReadInt16());
            }
            return res;
        }
        public static byte[] Serialize(this IEnumerable<float> collection)
        {
            var ds = new DataStorage();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static IEnumerable<float> FillDerialize(this IEnumerable<float> collection, byte[] buffer)
        {
            var ds = new DataStorage(buffer);
            int len = ds.ReadInt32();
            List<float> res = new List<float>();
            for (int i = 0; i < len; i++)
            {
                res.Add(ds.ReadFloat());
            }
            return res;
        }
        public static byte[] Serialize(this IEnumerable<double> collection)
        {
            var ds = new DataStorage();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static IEnumerable<double> FillDerialize(this IEnumerable<double> collection, byte[] buffer)
        {
            var ds = new DataStorage(buffer);
            int len = ds.ReadInt32();
            List<double> res = new List<double>();
            for (int i = 0; i < len; i++)
            {
                res.Add(ds.ReadDouble());
            }
            return res;
        }
        public static byte[] Serialize(this IEnumerable<string> collection)
        {
            var ds = new DataStorage();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static IEnumerable<string> FillDerialize(this IEnumerable<string> collection, byte[] buffer)
        {
            var ds = new DataStorage(buffer);
            int len = ds.ReadInt32();
            List<string> res = new List<string>();
            for (int i = 0; i < len; i++)
            {
                res.Add(ds.ReadString());
            }
            return res;
        }
        public static byte[] Serialize(this IEnumerable<char> collection)
        {
            var ds = new DataStorage();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static IEnumerable<char> FillDerialize(this IEnumerable<char> collection, byte[] buffer)
        {
            var ds = new DataStorage(buffer);
            int len = ds.ReadInt32();
            List<char> res = new List<char>();
            for (int i = 0; i < len; i++)
            {
                res.Add(ds.ReadChar());
            }
            return res;
        }
#endregion
    }
}