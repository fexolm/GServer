using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GServer.Containers
{
    public static class CodeGen
    {
        private static void SerializeListTo<TData>(DataStorage ds, IList<TData> list) {
            ds.Push(list.Count);
            foreach (var element in list) {
                DsSerializer.SerializeTo(ds, element);
            }
        }

        private static IList<TData> DeserializeListFrom<TData>(DataStorage ds) {
            int len = ds.ReadInt32();
            var result = new List<TData>();
            for (var i = 0; i < len; i++) {
                var element = DsSerializer.DeserializeFrom<TData>(ds);
                result.Add(element);
            }
            return result;
        }

        private static bool TrueIfNull(object obj) {
            return obj == null;
        }

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

        public static Action<DataStorage, object> GenerateSerializer(Type type) {
            Type[] @params = {typeof(DataStorage), typeof(object)};
            var method = new DynamicMethod("Serialize", typeof(void), @params);
            var il = method.GetILGenerator(256);
            il.Emit(OpCodes.Nop);
            PushSerializeMethods(il, type);
            il.Emit(OpCodes.Ret);
            try {
                return (Action<DataStorage, object>) method.CreateDelegate(typeof(Action<DataStorage, object>));
            }
            catch (Exception ex) {
                return null;
            }
        }

        private static void PushSerializeMethods(ILGenerator il, Type type) {
            var props = type.GetProperties()
                .Where(m => m.GetCustomAttribute(typeof(DsSerializeAttribute), false) != null);

            foreach (var prop in props) {
                GeneratePropertySerializer(il, prop);
            }
        }

        private static void GeneratePropertySerializer(ILGenerator il, PropertyInfo prop) {
            var elseStmt = il.DefineLabel();
            var endIf = il.DefineLabel();
            var options = prop.GetCustomAttribute<DsSerializeAttribute>().Options;
            if ((options & DsSerializeAttribute.SerializationOptions.Optional) != 0) {
                var trueIfNull = typeof(CodeGen).GetMethod("TrueIfNull", BindingFlags.NonPublic | BindingFlags.Static);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, prop.GetMethod);
                il.Emit(OpCodes.Call, trueIfNull);

                il.Emit(OpCodes.Call, _serializeActions[typeof(bool)]);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, prop.GetMethod);
                il.Emit(OpCodes.Call, trueIfNull);

                il.Emit(OpCodes.Brfalse, elseStmt);
                il.Emit(OpCodes.Br, endIf);
            }

            il.MarkLabel(elseStmt);
            if (_serializeActions.ContainsKey(prop.PropertyType)) {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, prop.GetMethod);
                var push = _serializeActions[prop.PropertyType];
                il.Emit(OpCodes.Callvirt, push);
                il.Emit(OpCodes.Pop);
            }
            else if (typeof(IList).IsAssignableFrom(prop.PropertyType)) {
                var listSerializer =
                    typeof(CodeGen).GetMethod("SerializeListTo", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(prop.PropertyType.GenericTypeArguments[0]);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, prop.GetMethod);
                il.Emit(OpCodes.Call, listSerializer);
            }
            else {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, prop.GetMethod);
                var serializer = typeof(DsSerializer).GetMethod("SerializeTo");
                il.Emit(OpCodes.Call, serializer);
            }

            il.MarkLabel(endIf);
        }

        public static Func<DataStorage, object> GenerateDeserializer(Type type) {
            Type[] @params = {typeof(DataStorage)};
            var method = new DynamicMethod("Deserialize", typeof(object), @params);
            var createDs = typeof(DataStorage).GetMethod("CreateForRead");
            var il = method.GetILGenerator(256);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.DeclareLocal(typeof(DataStorage));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Newobj, type.GetConstructor(new Type[0]));
            il.DeclareLocal(type);
            il.Emit(OpCodes.Stloc_1);
            PushDeserializeMethods(il, type);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
            try {
                return (Func<DataStorage, object>) method.CreateDelegate(typeof(Func<DataStorage, object>));
            }
            catch (Exception ex) {
                return null;
            }
        }

        private static void PushDeserializeMethods(ILGenerator il, Type type) {
            var props = type.GetProperties()
                .Where(m => m.GetCustomAttribute(typeof(DsSerializeAttribute), false) != null);
            foreach (var prop in props) {
                var options = prop.GetCustomAttribute<DsSerializeAttribute>().Options;
                var elseStmt = il.DefineLabel();
                var endIf = il.DefineLabel();
                if ((options & DsSerializeAttribute.SerializationOptions.Optional) != 0) {
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Call, _deserializeActions[typeof(bool)]);
                    il.Emit(OpCodes.Brfalse, elseStmt);
                    il.Emit(OpCodes.Br, endIf);
                }
                il.MarkLabel(elseStmt);
                if (_deserializeActions.ContainsKey(prop.PropertyType)) {
                    var read = _deserializeActions[prop.PropertyType];
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Call, read);
                    il.Emit(OpCodes.Callvirt, prop.SetMethod);
                }
                else if (typeof(IList).IsAssignableFrom(prop.PropertyType)) {
                    var listDeserializer =
                        typeof(CodeGen).GetMethod("DeserializeListFrom", BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(prop.PropertyType.GenericTypeArguments[0]);
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Call, listDeserializer);
                    il.Emit(OpCodes.Callvirt, prop.SetMethod);
                }
                else {
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    var deserializer = typeof(DsSerializer).GetMethod("DeserializeFrom")
                        .MakeGenericMethod(prop.PropertyType);
                    il.Emit(OpCodes.Call, deserializer);
                    il.Emit(OpCodes.Callvirt, prop.SetMethod);
                }
                il.MarkLabel(endIf);
            }
        }

        public static byte[] SerializeTest(IEnumerable<int> obj) {
            var ds = DataStorage.CreateForWrite();
            ds.Push(obj.Count());
            foreach (var o in obj) {
                ds.Push(o);
            }
            return ds.Serialize();
        }

        public static void SerializeIenumerable(ILGenerator il, PropertyInfo prop, Action getPropAction) { }
    }
}
