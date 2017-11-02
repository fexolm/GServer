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

        public static Action<DataStorage, object> GenerateSerializer(Type type) {
            Type[] @params = {typeof(DataStorage), typeof(object)};
            var method = new DynamicMethod("Serialize", typeof(void), @params);
            var createDs =
                typeof(DataStorage).GetMethod("CreateForWrite", BindingFlags.Public | BindingFlags.Static);
            var serialize = typeof(DataStorage).GetMethod("Serialize");
            var il = method.GetILGenerator(256);
            il.Emit(OpCodes.Nop);

            PushSerializeMethods(il, type, (prop) => {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, prop.GetMethod);
            });
            il.Emit(OpCodes.Ret);
            try {
                return (Action<DataStorage, object>) method.CreateDelegate(typeof(Action<DataStorage, object>));
            }
            catch (Exception ex) {
                return null;
            }
        }

        private static void PushSerializeMethods(ILGenerator il, Type type, Action<PropertyInfo> getPropAction) {
            var props = type.GetProperties()
                .Where(m => m.GetCustomAttribute(typeof(DsSerializeAttribute), false) != null);

            foreach (var prop in props) {
                GeneratePropertySerializer(il, prop, getPropAction);
            }
        }

        private static void GeneratePropertySerializer(ILGenerator il, PropertyInfo prop,
            Action<PropertyInfo> getPropAction) {
            var elseStmt = il.DefineLabel();
            var endIf = il.DefineLabel();
            var options = prop.GetCustomAttribute<DsSerializeAttribute>().Options;
            if ((options & DsSerializeAttribute.SerializationOptions.Optional) != 0) {
                il.Emit(OpCodes.Ldarg_0);
                getPropAction.Invoke(prop);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Brfalse, elseStmt);
                il.Emit(OpCodes.Br, endIf);
            }

            il.MarkLabel(elseStmt);
            if (_serializeActions.ContainsKey(prop.PropertyType)) {
                il.Emit(OpCodes.Ldarg_0);
                getPropAction.Invoke(prop);
                var push = _serializeActions[prop.PropertyType];
                il.Emit(OpCodes.Callvirt, push);
                il.Emit(OpCodes.Pop);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)) { }
            else {
                PushSerializeMethods(il, prop.PropertyType, (p) => {
                    getPropAction.Invoke(prop);
                    il.Emit(OpCodes.Callvirt, p.GetMethod);
                });
            }

            il.MarkLabel(endIf);

            if ((options & DsSerializeAttribute.SerializationOptions.Optional) == 0) return;
            il.Emit(OpCodes.Not);
            il.Emit(OpCodes.Call, _serializeActions[typeof(bool)]);
            il.Emit(OpCodes.Pop);
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
            PushDeserializeMethods(il, type, () => { il.Emit(OpCodes.Ldloc_1); });
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
            try {
                return (Func<DataStorage, object>) method.CreateDelegate(typeof(Func<DataStorage, object>));
            }
            catch (Exception ex) {
                return null;
            }
        }

        private static void PushDeserializeMethods(ILGenerator il, Type type, Action getPropAction) {
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
                    getPropAction.Invoke();
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Call, read);
                    il.Emit(OpCodes.Callvirt, prop.SetMethod);
                }
                else {
                    getPropAction.Invoke();
                    il.Emit(OpCodes.Newobj, prop.PropertyType.GetConstructor(new Type[0]));
                    il.Emit(OpCodes.Callvirt, prop.SetMethod);
                    PushDeserializeMethods(il, prop.PropertyType, () => {
                        getPropAction.Invoke();
                        il.Emit(OpCodes.Callvirt, prop.GetMethod);
                    });
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