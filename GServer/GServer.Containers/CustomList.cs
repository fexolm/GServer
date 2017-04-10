using System;
using System.Collections;
using System.Collections.Generic;

namespace GServer
{
    public class CustomNode<TData>
    {
        public TData Value { get; set; }
        public CustomNode<TData> Next { get; internal set; }
        public CustomNode<TData> Prev { get; internal set; }
    }

    public class CustomList<TData> : IEnumerable<TData>, IEnumerable<CustomNode<TData>>
    {
        private CustomNode<TData> _head;
        private CustomNode<TData> _tail;
        private Enumerator _enumerator;

        public bool Empty => _head.Next == _tail;

        public CustomNode<TData> First => _head.Next;

        public CustomNode<TData> Last => _tail.Prev;
        public CustomList()
        {
            _head = new CustomNode<TData>();
            _tail = new CustomNode<TData>();
            _head.Next = _tail;
            _tail.Prev = _head;
            _enumerator = new Enumerator(this);
        }

        public CustomList<TData> PushBack(TData val)
        {
            var node = new CustomNode<TData>();
            node.Value = val;
            node.Prev = _tail.Prev;
            node.Next = _tail;
            _tail.Prev.Next = node;
            _tail.Prev = node;
            return this;
        }

        public CustomList<TData> PushFront(TData val)
        {
            var node = new CustomNode<TData>();
            node.Value = val;
            node.Prev = _head;
            node.Next = _head.Next;
            _head.Next.Prev = node;
            _head.Next = node;
            return this;
        }

        public CustomList<TData> InsertAfter(CustomNode<TData> after, TData val)
        {
            var node = new CustomNode<TData>();
            node.Value = val;
            node.Next = after.Next;
            node.Prev = after;
            after.Next.Prev = node;
            after.Next = node;
            return this;
        }

        public int Count
        {
            get
            {
                int len = 0;
                var node = First;
                while (node != _tail)
                {
                    len++;
                    node = node.Next;
                }
                return len;
            }
        }

        public CustomList<TData> InsertBefore(CustomNode<TData> before, TData val)
        {
            var node = new CustomNode<TData>();
            node.Value = val;
            node.Next = before;
            node.Prev = before.Prev;
            before.Prev.Next = node;
            before.Prev = node;
            return this;
        }
        public CustomList<TData> RemoveBetween(CustomNode<TData> begin, CustomNode<TData> end)
        {
            begin.Next = end;
            end.Prev = begin;
            return this;
        }
        public override string ToString()
        {
            string result = "";
            foreach (var element in this)
            {
                result += element + " ";
            }
            return result;
        }
        public IEnumerator<TData> GetEnumerator()
        {
            return _enumerator;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerator;
        }
        IEnumerator<CustomNode<TData>> IEnumerable<CustomNode<TData>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        private struct Enumerator : IEnumerator<TData>
        {
            private CustomList<TData> _list;
            private CustomNode<TData> _current;

            public Enumerator(CustomList<TData> list)
            {
                _list = list;
                _current = list._head;
            }

            public TData Current => _current.Value;

            object IEnumerator.Current => _current.Value;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_current.Next != _list._tail)
                {
                    _current = _current.Next;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                _current = _list._head;
            }
        }
    }

}
