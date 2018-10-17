using System;
using System.Collections;
using System.Collections.Generic;

namespace Freedom.FullTextSearch
{
    public abstract class IndexKeySet
    {
        public abstract Type EntityType { get; }
    }

    public class IndexKeySet<T> : IndexKeySet, ICollection<Guid>
    {
        private readonly HashSet<Guid> _hashSet = new HashSet<Guid>();

        public override Type EntityType => typeof(T);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _hashSet.GetEnumerator();
        }

        public IEnumerator<Guid> GetEnumerator()
        {
            return _hashSet.GetEnumerator();
        }

        public void Add(Guid item)
        {
            _hashSet.Add(item);
        }

        public void Clear()
        {
            _hashSet.Clear();
        }

        public bool Contains(Guid item)
        {
            return _hashSet.Contains(item);
        }

        public void CopyTo(Guid[] array, int arrayIndex)
        {
            _hashSet.CopyTo(array, arrayIndex);
        }

        public bool Remove(Guid item)
        {
            return _hashSet.Remove(item);
        }

        public int Count => _hashSet.Count;

        public bool IsReadOnly => false;
    }

}
