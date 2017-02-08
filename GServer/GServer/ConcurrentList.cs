using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GServer
{
    class ConcurrentList<T> : IEnumerable<T>, IEnumerator<T>
    {
        private readonly List<T> _list;
        private readonly Mutex _mutex;

        public ConcurrentList()
        {
            _list = new List<T>();
            _mutex = new Mutex();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public T Current
        {
            get
            {
                return GetEnumerator().Current;
            }
        }


        public void Dispose()
        {
            foreach (var element in _list)
            {
                if (element is IDisposable)
                    ((IDisposable)element).Dispose();
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return GetEnumerator().Current;
            }
        }

        public bool MoveNext()
        {
            return GetEnumerator().MoveNext();
        }

        public void Reset()
        {
            GetEnumerator().Reset();
        }

        public void Add(T value)
        {
            _mutex.WaitOne();
            _list.Add(value);
            _mutex.ReleaseMutex();
        }
        public void Remove(T value)
        {
            _mutex.WaitOne();
            _list.Remove(value);
            _mutex.ReleaseMutex();
        }
        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }
    }
}
