using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GServer
{
    public class ConcurrentQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly Mutex _mutex;
        public ConcurrentQueue()
        {
            _queue = new Queue<T>();
            _mutex = new Mutex();
        }
        public void Enqueue(T val)
        {
            _mutex.WaitOne();
            _queue.Enqueue(val);
            _mutex.ReleaseMutex();
        }
        public T Dequeue()
        {
            _mutex.WaitOne();
            var result = _queue.Dequeue();
            _mutex.ReleaseMutex();
            return result;
        }
        public int Count { get { return _queue.Count; } }
        public T Peek()
        {
            return _queue.Peek();
        }
    }
}
