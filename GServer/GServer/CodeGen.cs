using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Linq;
using GServer.Containers;


namespace GServer
{
    public static class CodeGen
    {
        public delegate byte[] Serializer(object obj);

        private static readonly IDictionary<Type, MethodInfo> _serializeActions =
            new Dictionary<Type, MethodInfo>();

        private static readonly IDictionary<Type, MethodInfo> _deserializeActions =
            new Dictionary<Type, MethodInfo>();


        static CodeGen() {
            _serializeActions.Add(typeof(byte),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(byte)}));

            _serializeActions.Add(typeof(short),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(short)}));

            _serializeActions.Add(typeof(int),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(int)}));

            _serializeActions.Add(typeof(long),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(long)}));

            _serializeActions.Add(typeof(decimal),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(decimal)}));

            _serializeActions.Add(typeof(bool),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(bool)}));

            _serializeActions.Add(typeof(char),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(char)}));

            _serializeActions.Add(typeof(string),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(string)}));

            _serializeActions.Add(typeof(double),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(double)}));

            _serializeActions.Add(typeof(float),
                typeof(DataStorage).GetMethod("Push", new[] {typeof(float)}));

            _deserializeActions.Add(typeof(byte),
                typeof(DataStorage).GetMethod("ReadByte", new Type[0]));

            _deserializeActions.Add(typeof(short),
                typeof(DataStorage).GetMethod("ReadInt16", new Type[0]));

            _deserializeActions.Add(typeof(int),
                typeof(DataStorage).GetMethod("ReadInt32", new Type[0]));

            _deserializeActions.Add(typeof(long),
                typeof(DataStorage).GetMethod("ReadInt64", new Type[0]));

            _deserializeActions.Add(typeof(decimal),
                typeof(DataStorage).GetMethod("ReadDecimal", new Type[0]));

            _deserializeActions.Add(typeof(bool),
                typeof(DataStorage).GetMethod("ReadBoolean", new Type[0]));

            _deserializeActions.Add(typeof(char),
                typeof(DataStorage).GetMethod("ReadChar", new Type[0]));

            _deserializeActions.Add(typeof(string),
                typeof(DataStorage).GetMethod("ReadString", new Type[0]));

            _deserializeActions.Add(typeof(double),
                typeof(DataStorage).GetMethod("ReadDouble", new Type[0]));

            _deserializeActions.Add(typeof(float),
                typeof(DataStorage).GetMethod("ReadFloat", new Type[0]));
        }

        public static Func<object, byte[]> GenerateSerializer(Type type) {
            Type[] @params = {typeof(object)};
            var method = new DynamicMethod("Serialize", typeof(byte[]), @params);
            var createDs =
                typeof(DataStorage).GetMethod("CreateForWrite", BindingFlags.Public | BindingFlags.Static);
            var serialize = typeof(DataStorage).GetMethod("Serialize");
            var il = method.GetILGenerator(256);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Call, createDs);
            il.DeclareLocal(typeof(DataStorage));
            il.Emit(OpCodes.Stloc_0);

            PushSerializeMethods(il, type);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, serialize);
            il.Emit(OpCodes.Ret);
            return (Func<object, byte[]>) method.CreateDelegate(typeof(Func<object, byte[]>));
        }

        private static void PushSerializeMethods(ILGenerator il, Type type) {
            var props = type.GetProperties()
                .Where(m => m.GetCustomAttribute(typeof(DsSerializeAttribute), false) != null);

            foreach (var prop in props) {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, prop.GetMethod);
                var options = prop.GetCustomAttribute<DsSerializeAttribute>().Options;
                if ((options & DsSerializeAttribute.SerializationOptions.Optional) != 0) {
                    // generate bool
                }
                if (_serializeActions.ContainsKey(prop.PropertyType)) {
                    var push = _serializeActions[prop.PropertyType];
                    il.Emit(OpCodes.Callvirt, push);
                    il.Emit(OpCodes.Pop);
                }
                else {
                    PushSerializeMethods(il, prop.PropertyType);
                }
            }
        }

        public static Func<byte[], object> GenerateDeserializer(Type type) {
            Type[] @params = {typeof(byte[])};
            var method = new DynamicMethod("Deserialize", typeof(object), @params);
            var createDs = typeof(DataStorage).GetMethod("CreateForRead");

            var il = method.GetILGenerator(256);

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, createDs);
            il.DeclareLocal(typeof(DataStorage));
            il.Emit(OpCodes.Stloc_0);

            PushDeserializeMethods(il, type);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
            return (Func<byte[], object>) method.CreateDelegate(typeof(Func<byte[], object>));
        }

        private static void PushDeserializeMethods(ILGenerator il, Type type) {
            var ctor = type.GetConstructor(new Type[0]);
            il.Emit(OpCodes.Newobj, ctor);
            il.DeclareLocal(type);
            il.Emit(OpCodes.Stloc_1);

            var props = type.GetProperties()
                .Where(m => m.GetCustomAttribute(typeof(DsSerializeAttribute), false) != null);


            foreach (var prop in props) {
                var options = prop.GetCustomAttribute<DsSerializeAttribute>().Options;
                if ((options & DsSerializeAttribute.SerializationOptions.Optional) != 0) {
                    // read bool
                }
                if (_deserializeActions.ContainsKey(prop.PropertyType)) {
                    var read = _deserializeActions[prop.PropertyType];
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Call, read);
                    il.Emit(OpCodes.Callvirt, prop.SetMethod);
                }
                else {
                    PushDeserializeMethods(il, prop.PropertyType);
                }
            }
        }

        public class SerializingClass
        {
            public int Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        public static object DerializeClass(byte[] buf) {
            var ds = DataStorage.CreateForRead(buf);
            var c = new SerializingClass();
            c.Prop1 = ds.ReadInt32();
            c.Prop2 = ds.ReadString();
            return c;
        }
    }
}