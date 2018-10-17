using System.Collections;
using System.Collections.Generic;

namespace Freedom.Collections
{
    public class ConcurrentHashSet<TObject> : ICollection<TObject>
    {
        private readonly object _lock = new object();
        private readonly HashSet<TObject> _hashSet = new HashSet<TObject>();

        public IEnumerator<TObject> GetEnumerator()
        {
            IEnumerable<TObject> clone;

            lock (_lock)
            {
                clone = new TObject[_hashSet.Count];

                _hashSet.CopyTo((TObject[]) clone);
            }

            return clone.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Add(TObject item)
        {
            lock (_lock)
            {
                return _hashSet.Add(item);
            }
        }

        void ICollection<TObject>.Add(TObject item)
        {
            Add(item);
        }

        public void Clear()
        {
            lock (_lock)
            {
                _hashSet.Clear();
            }
        }

        public bool Contains(TObject item)
        {
            lock (_lock)
            {
                return _hashSet.Contains(item);
            }
        }

        public void CopyTo(TObject[] array, int arrayIndex)
        {
            lock (_lock)
            {
                _hashSet.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(TObject item)
        {
            lock (_lock)
            {
                return _hashSet.Remove(item);
            }
        }

        public int Count => _hashSet.Count;

        public bool IsReadOnly => false;

        public object SyncRoot => _lock;
    }
}

