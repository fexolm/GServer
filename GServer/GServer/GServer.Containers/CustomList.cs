using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable ArrangeAccessorOwnerBody

namespace GServer.Containers
{
    public class CustomNode<TData>
    {
        public TData Value { get; set; }
        public CustomNode<TData> Next { get; internal set; }
        public CustomNode<TData> Prev { get; internal set; }
    }

    public class CustomList<TData> : IEnumerable<TData>, IEnumerable<CustomNode<TData>>
    {
        private readonly CustomNode<TData> _head;
        private readonly CustomNode<TData> _tail;
        private readonly Enumerator _enumerator;

        public bool Empty {
            get { return _head.Next == _tail; }
        }

        public CustomNode<TData> First {
            get { return _head.Next; }
        }

        public CustomNode<TData> Last {
            get { return _tail.Prev; }
        }

        public CustomList() {
            _head = new CustomNode<TData>();
            _tail = new CustomNode<TData>();
            _head.Next = _tail;
            _tail.Prev = _head;
            _enumerator = new Enumerator(this);
        }

        public CustomList<TData> PushBack(TData val) {
            var node = new CustomNode<TData> {
                Value = val,
                Prev = _tail.Prev,
                Next = _tail
            };
            _tail.Prev.Next = node;
            _tail.Prev = node;
            return this;
        }

        public CustomList<TData> PushFront(TData val) {
            var node = new CustomNode<TData> {
                Value = val,
                Prev = _head,
                Next = _head.Next
            };
            _head.Next.Prev = node;
            _head.Next = node;
            return this;
        }

        public CustomList<TData> InsertAfter(CustomNode<TData> after, TData val) {
            var node = new CustomNode<TData> {
                Value = val,
                Next = after.Next,
                Prev = after
            };
            after.Next.Prev = node;
            after.Next = node;
            return this;
        }

        public int Count {
            get {
                var len = 0;
                var node = First;
                while (node != _tail) {
                    len++;
                    node = node.Next;
                }
                return len;
            }
        }

        public CustomList<TData> InsertBefore(CustomNode<TData> before, TData val) {
            var node = new CustomNode<TData> {
                Value = val,
                Next = before,
                Prev = before.Prev
            };
            before.Prev.Next = node;
            before.Prev = node;
            return this;
        }

        public CustomList<TData> RemoveBetween(CustomNode<TData> begin, CustomNode<TData> end) {
            begin.Next = end;
            end.Prev = begin;
            return this;
        }

        public override string ToString() {
            var result = "";
            foreach (var element in this) {
                result += element + " ";
            }
            return result;
        }

        public IEnumerator<TData> GetEnumerator() {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _enumerator;
        }

        IEnumerator<CustomNode<TData>> IEnumerable<CustomNode<TData>>.GetEnumerator() {
            throw new NotImplementedException();
        }

        private struct Enumerator : IEnumerator<TData>
        {
            private readonly CustomList<TData> _list;
            private CustomNode<TData> _current;

            public Enumerator(CustomList<TData> list) {
                _list = list;
                _current = list._head;
            }

            public TData Current {
                get { return _current.Value; }
            }

            object IEnumerator.Current {
                get { return _current.Value; }
            }

            public void Dispose() { }

            public bool MoveNext() {
                if (_current.Next == _list._tail) return false;
                _current = _current.Next;
                return true;
            }

            public void Reset() {
                _current = _list._head;
            }
        }
    }
}