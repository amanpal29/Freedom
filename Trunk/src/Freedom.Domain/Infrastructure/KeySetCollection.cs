using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Freedom.Domain.Infrastructure
{
    public interface IKeySetCollection : ICollection<Guid>
    {
        void AddRange(ICollection<Guid> keys);

        void ReplaceWith(ICollection<Guid> keys);

        void ReplaceWith(DbContext context, IEnumerable<Guid> keys);
    }

    public class KeySetCollection<T> : IKeySetCollection
        where T : class, new()
    {
        private readonly ICollection<T> _intermediate;

        private readonly Func<T, Guid> _getKey;
        private readonly Action<T, Guid> _setKey;

        public KeySetCollection(ICollection<T> intermediate, Expression<Func<T, Guid>> keyPropertyExpression)
        {
            _intermediate = intermediate;

            _getKey = keyPropertyExpression.Compile();

            _setKey = DelegateFactory.PropertySetAction(keyPropertyExpression);
        }

        public IEnumerator<Guid> GetEnumerator()
        {
            return _intermediate.Select(_getKey).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Guid key)
        {
            if (Contains(key)) return;

            T item = new T();

            _setKey(item, key);

            _intermediate.Add(item);
        }

        public void AddRange(ICollection<Guid> keys)
        {
            foreach (Guid key in keys)
                Add(key);
        }

        public void ReplaceWith(ICollection<Guid> keys)
        {
            if (keys == null || keys.Count == 0)
            {
                _intermediate.Clear();               
            }
            else
            {
                List<T> itemsToRemove = _intermediate.ToList();

                foreach (Guid key in keys)
                {
                    if (itemsToRemove.RemoveAll(x => _getKey(x) == key) > 0)
                        continue;

                    T item = new T();

                    _setKey(item, key);

                    _intermediate.Add(item);
                }

                foreach (T item in itemsToRemove)
                {
                    _intermediate.Remove(item);
                }
            }
        }

        public void ReplaceWith(DbContext context, IEnumerable<Guid> keys)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            DbSet<T> dbSet = context.Set<T>();

            _intermediate.Clear();

            foreach (Guid key in keys)
            {
                T item = dbSet.Create();

                _setKey(item, key);

                _intermediate.Add(item);

                dbSet.Attach(item);
            }
        }

        public void Clear()
        {
            _intermediate.Clear();
        }

        public bool Contains(Guid key)
        {
            return _intermediate.Any(x => _getKey(x) == key);
        }

        public void CopyTo(Guid[] array, int arrayIndex)
        {
            foreach (T item in _intermediate)
                array[arrayIndex++] = _getKey(item);
        }

        public bool Remove(Guid value)
        {
            List<T> itemsToRemove = _intermediate.Where(x => _getKey(x) == value).ToList();

            foreach (T item in itemsToRemove)
                _intermediate.Remove(item);

            return itemsToRemove.Count > 0;
        }

        public int Count => _intermediate.Count;

        public bool IsReadOnly => false;
    }
}
