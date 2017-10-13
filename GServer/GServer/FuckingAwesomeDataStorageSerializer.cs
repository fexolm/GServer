// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local

using System;
using System.Collections.Generic;

namespace GServer
{
    public static class FuckingAwesomeDataStorageSerializer
    {
        private static readonly IDictionary<Type, Func<object, byte[]>> _serializerCache
            = new Dictionary<Type, Func<object, byte[]>>();

        private static readonly IDictionary<Type, Func<byte[], object>> _deserializerCache
            = new Dictionary<Type, Func<byte[], object>>();

        public static byte[] Serialize(object obj) {
            if (!_serializerCache.ContainsKey(obj.GetType())) {
                _serializerCache[obj.GetType()] = CodeGen.GenerateSerializer(obj.GetType());
            }
            return _serializerCache[obj.GetType()].Invoke(obj);
        }

        public static TResult DeserializeInto<TResult>(byte[] buffer) {
            if (!_deserializerCache.ContainsKey(typeof(TResult))) {
                _deserializerCache[typeof(TResult)] = CodeGen.GenerateDeserializer(typeof(TResult));
            }
            return (TResult) _deserializerCache[typeof(TResult)].Invoke(buffer);
        }
    }
}