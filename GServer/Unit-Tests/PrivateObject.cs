using System;
using System.Collections.Generic;
using System.Linq;
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
            return _type.GetMethod("methodName").Invoke(_object, parameters);
        }
        public object GetField(string fieldName)
        {
            return _type.GetField(fieldName).GetValue(_object);
        }
        public void SetField(string fieldName, object value)
        {
            _type.GetField(fieldName).SetValue(_object, value);
        }
    }
}
