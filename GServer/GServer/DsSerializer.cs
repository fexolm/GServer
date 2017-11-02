// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local

using System;
using System.Collections.Generic;
using System.Net;
using GServer.Containers;

namespace GServer
{
    public static class DsSerializer
    {
        private static readonly IDictionary<Type, Action<DataStorage, object>> _serializerCache
            = new Dictionary<Type, Action<DataStorage, object>>();

        private static readonly IDictionary<Type, Func<DataStorage, object>> _deserializerCache
            = new Dictionary<Type, Func<DataStorage, object>>();

        public static void SerializeTo(DataStorage ds, object obj) {
            if (!_serializerCache.ContainsKey(obj.GetType())) {
                _serializerCache[obj.GetType()] = CodeGen.GenerateSerializer(obj.GetType());
            }
            _serializerCache[obj.GetType()].Invoke(ds, obj);
        }

        public static TResult DeserializeFrom<TResult>(DataStorage ds) {
            if (!_deserializerCache.ContainsKey(typeof(TResult))) {
                _deserializerCache[typeof(TResult)] = CodeGen.GenerateDeserializer(typeof(TResult));
            }
            return (TResult) _deserializerCache[typeof(TResult)].Invoke(ds);
        }

        public static byte[] Serialize(object obj) {
            var ds = DataStorage.CreateForWrite();
            SerializeTo(ds, obj);
            return ds.Serialize();
        }

        public static TResult DeserializeInto<TResult>(byte[] buffer) {
            var ds = DataStorage.CreateForRead(buffer);
            return DeserializeFrom<TResult>(ds);
        }
    }
}