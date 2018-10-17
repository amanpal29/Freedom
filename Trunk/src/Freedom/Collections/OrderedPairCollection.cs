using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Freedom.Collections
{
    public class OrderedPairCollection<T1, T2> : IList<Tuple<T1, T2>>
    {
        private readonly IList<Tuple<T1, T2>> _items;
        private readonly int? _capacity;

        public OrderedPairCollection(int? capacity = null)
        {
            _capacity = capacity;
            _items = capacity.HasValue ? new List<Tuple<T1, T2>>(capacity.Value) : new List<Tuple<T1, T2>>();
        }

        public OrderedPairCollection(IEnumerable<Tuple<T1, T2>> items, int? capacity = null)
        {
            _items = items.ToList();

            if (_items.Count > capacity)
            {
                throw new ArgumentException("number of items is greater than maximumSize");
            }

            _capacity = capacity;
        }

        public OrderedPairCollection(IEnumerable<T1> firstItems, IEnumerable<T2> secondItems, int? capacity = null)
        {
            List<T1> firstItemsList = firstItems.ToList();
            List<T2> secondItemsList = secondItems.ToList();

            if (firstItemsList.Count != secondItemsList.Count)
            {
                throw new ArgumentException("firstItems and secondItems must contain the same number of values");
            }

            if (capacity.HasValue && (firstItemsList.Count > capacity || secondItemsList.Count > capacity))
            {
                throw new ArgumentException("number of items is greater than maximumSize");
            }

            _capacity = capacity;

            for (int i = 0; i < firstItemsList.Count; i++)
            {
                Add(new Tuple<T1, T2>(firstItemsList.ElementAt(i), secondItemsList.ElementAt(i)));
            }
        }

        public void Add(Tuple<T1, T2> item)
        {
            if (_capacity.HasValue && _items.Count == _capacity)
            {
                throw new InvalidOperationException($"maximum size of collection ({_capacity}) exceeded");
            }

            _items.Add(item);
        }

        public void Add(T1 item1, T2 item2)
        {
            if (_capacity.HasValue && _items.Count == _capacity)
            {
                throw new InvalidOperationException($"maximum size of collection ({_capacity}) exceeded");
            }

            Add(new Tuple<T1, T2>(item1, item2));
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(Tuple<T1, T2> item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(Tuple<T1, T2> [] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(Tuple<T1, T2> item)
        {
            return _items.Remove(item);
        }

        public int IndexOf(Tuple<T1, T2> item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, Tuple<T1, T2> item)
        {
            _items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public void Replace(int index, Tuple<T1, T2> item)
        {
            RemoveAt(index);
            Insert(index, item);
        }

        public Tuple<T1, T2> this[int index]
        {
            get
            {
                if (_items.Count - 1 < index)
                {
                    return null;
                }

                return _items.ElementAt(index);
            }
            set { Replace(index, value); }
        }

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public IEnumerator<Tuple<T1, T2>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (Tuple<T1, T2> item in _items)
            {
                builder.Append($"{item.Item1},{item.Item2};");
            }

            return builder.ToString();
        }
    }
}
