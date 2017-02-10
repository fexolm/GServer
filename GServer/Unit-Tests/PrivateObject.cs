using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Unit_Tests
{
    internal class PrivateObject
    {
        private readonly object _object;
        private readonly Type _type;
        public PrivateObject(object obj)
        {
            _object = obj;
            _type = _object.GetType();
        }

        public object Invoke(string methodName, params object[] parameters)
        {
            return _type.GetMethod(methodName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance).Invoke(_object, parameters);
        }
        public object GetField(string fieldName)
        {
            return _type.GetField(fieldName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance).GetValue(_object);
        }
        public void SetField(string fieldName, object value)
        {
            _type.GetField(fieldName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance).SetValue(_object, value);
        }
        public object GetProperty(string propName)
        {
            return _type.GetProperty(propName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance).GetValue(_object, null);
        }
        public void SetProperty(string propName, object value)
        {
            _type.GetProperty(propName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance).SetValue(_object, value, null);
        }
    }
}
