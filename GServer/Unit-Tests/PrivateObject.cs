using System;
using System.Reflection;

namespace Unit_Tests
{
    internal class PrivateObject
    {
        private readonly object _object;
        private readonly Type _type;

        public PrivateObject(object obj) {
            _object = obj;
            _type = _object.GetType();
        }

        public object Invoke(string methodName, params object[] parameters) {
            return _type.GetMethod(methodName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance).Invoke(_object, parameters);
        }

        public object GetField(string fieldName) {
            var memberInfo = _type.GetField(fieldName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            return memberInfo != null ? memberInfo.GetValue(_object) : null;
        }

        public void SetField(string fieldName, object value) {
            var memberInfo = _type.GetField(fieldName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            if (memberInfo != null)
                memberInfo.SetValue(_object, value);
        }

        public object GetProperty(string propName) {
            var propertyInfo = _type.GetProperty(propName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            return propertyInfo != null ? propertyInfo.GetValue(_object, null) : null;
        }

        public void SetProperty(string propName, object value) {
            var propertyInfo = _type.GetProperty(propName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            if (propertyInfo != null)
                propertyInfo.SetValue(_object, value, null);
        }
    }
}