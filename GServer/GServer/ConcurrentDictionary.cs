using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GServer
{
    public class ConcurrentDictionary<Tkey, TValue>
    {
        private readonly IDictionary<Tkey, TValue> _dictionary;
        private readonly Mutex _mutex;
        public ConcurrentDictionary()
        {
            _dictionary = new SortedDictionary<Tkey, TValue>();
        }
        public void Add(Tkey key, TValue value)
        {
            _mutex.WaitOne();
            _dictionary.Add(key, value);
            _mutex.ReleaseMutex();
        }
        public void Remove(Tkey key)
        {
            _mutex.WaitOne();
            _dictionary.Remove(key);
            _mutex.ReleaseMutex();
        }
        public TValue this[Tkey key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                _dictionary[key] = value;
            }
        }
    }
}
