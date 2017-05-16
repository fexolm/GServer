using System;
using System.Collections.Generic;
using System.Linq;
namespace GServer.Containers
{
    public static class CollectionExtensions
    {
        public static void Invoke<T>(this List<T> collection, Action<T> action)
        {
            foreach (var element in collection)
            {
                action.Invoke(element);
            }
        }

        #region List Serialization
        public static byte[] Serialize(this IEnumerable<int> collection)
        {
            var ds = DataStorage.CreateForWrite();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static void FillDerialize(this List<int> collection, byte[] buffer)
        {
            var ds = DataStorage.CreateForRead(buffer);
            int len = ds.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                collection.Add(ds.ReadInt32());
            }
        }
        public static byte[] Serialize(this IEnumerable<long> collection)
        {
            var ds = DataStorage.CreateForWrite();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static void FillDerialize(this List<long> collection, byte[] buffer)
        {
            var ds = DataStorage.CreateForRead(buffer);
            int len = ds.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                collection.Add(ds.ReadInt64());
            }
        }
        public static byte[] Serialize(this IEnumerable<short> collection)
        {
            var ds = DataStorage.CreateForWrite();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static void FillDerialize(this List<short> collection, byte[] buffer)
        {
            var ds = DataStorage.CreateForRead(buffer);
            int len = ds.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                collection.Add(ds.ReadInt16());
            }
        }
        public static byte[] Serialize(this IEnumerable<float> collection)
        {
            var ds = DataStorage.CreateForWrite();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static void FillDerialize(this List<float> collection, byte[] buffer)
        {
            var ds = DataStorage.CreateForRead(buffer);
            int len = ds.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                collection.Add(ds.ReadFloat());
            }

        }
        public static byte[] Serialize(this IEnumerable<double> collection)
        {
            var ds = DataStorage.CreateForWrite();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static void FillDerialize(this List<double> collection, byte[] buffer)
        {
            var ds = DataStorage.CreateForRead(buffer);
            int len = ds.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                collection.Add(ds.ReadDouble());
            }
        }
        public static byte[] Serialize(this IEnumerable<string> collection)
        {
            var ds = DataStorage.CreateForWrite();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static void FillDerialize(this List<string> collection, byte[] buffer)
        {
            var ds = DataStorage.CreateForRead(buffer);
            int len = ds.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                collection.Add(ds.ReadString());
            }
        }
        public static byte[] Serialize(this IEnumerable<char> collection)
        {
            var ds = DataStorage.CreateForWrite();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static void FillDerialize(this List<char> collection, byte[] buffer)
        {
            var ds = DataStorage.CreateForRead(buffer);
            int len = ds.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                collection.Add(ds.ReadChar());
            }
        }
        public static byte[] Serialize<TData>(this IEnumerable<TData> collection)
            where TData : IDeepSerializable
        {
            var ds = DataStorage.CreateForWrite();
            ds.Push(collection.Count());
            foreach (var element in collection)
            {
                ds.Push(element);
            }
            return ds.Serialize();
        }
        public static void FillDerialize<TData>(this List<TData> collection, byte[] buffer)
            where TData : IDeepDeserializable, new()
        {
            var ds = DataStorage.CreateForRead(buffer);
            int len = ds.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                TData element = new TData();
                element.ReadFromDs(ds);
                collection.Add(element);
            }
        }
        #endregion
    }
}