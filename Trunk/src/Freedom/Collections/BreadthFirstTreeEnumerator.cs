using System;
using System.Collections;
using System.Collections.Generic;

namespace Freedom.Collections
{
    public sealed class BreadthFirstTreeEnumerator<T> : IEnumerator<T>
        where T : IEnumerable<T>
    {
        private readonly Queue<T> _queue;

        public BreadthFirstTreeEnumerator(IEnumerable<T> rootNode)
        {
            _queue = new Queue<T>(rootNode);
        }

        public bool MoveNext()
        {
            if (_queue.Count == 0)
                return false;

            Current = _queue.Dequeue();

            foreach (T child in Current)
                _queue.Enqueue(child);

            return true;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}
