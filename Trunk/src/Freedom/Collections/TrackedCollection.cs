using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Freedom.Annotations;

namespace Freedom.Collections
{
    public class TrackedCollection<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string IndexerName = "Item[]";

        private readonly List<T> _items;
        private readonly IList<T> _itemsAddedToCollection;
        private readonly IList<T> _itemsRemovedFromCollection;

        public TrackedCollection()
        {
            _items = new List<T>();
            _itemsAddedToCollection = new List<T>();
            _itemsRemovedFromCollection = new List<T>();
        }

        public TrackedCollection(IEnumerable<T> existingItems)
            : this(existingItems, null, null)
        {
        }

        public TrackedCollection(IEnumerable<T> existingItems, IList<T> itemsAddedToCollection, IList<T> itemsRemovedFromCollection)
        {
            _items = new List<T>(existingItems);
            _itemsAddedToCollection = itemsAddedToCollection ?? new List<T>();
            _itemsRemovedFromCollection = itemsRemovedFromCollection ?? new List<T>();
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            int startIndex = _items.Count;

            foreach (T item in items)
            {
                if (_items.Contains(item)) return;

                if (!_itemsRemovedFromCollection.Remove(item))
                    _itemsAddedToCollection.Add(item);

                _items.Add(item);
            }

            int numberOfNewItems = _items.Count - startIndex;

            if (numberOfNewItems > 0)
            {
                T[] newItems = new T[numberOfNewItems];

                _items.CopyTo(startIndex, newItems, 0, numberOfNewItems);

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, startIndex));
            }
        }

        public IList<T> ItemsAddedToCollection => new ReadOnlyCollection<T>(_itemsAddedToCollection);

        public IList<T> ItemsRemovedToCollection => new ReadOnlyCollection<T>(_itemsRemovedFromCollection);

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<T>

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_items.Contains(item)) return;

            if (!_itemsRemovedFromCollection.Remove(item))
                _itemsAddedToCollection.Add(item);

            _items.Add(item);

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            if (_items.Count <= 0) return;

            foreach (T item in _itemsAddedToCollection)
                _items.Remove(item);

            foreach (T item in _items)
                if (!_itemsRemovedFromCollection.Contains(item))
                    _itemsRemovedFromCollection.Add(item);

            _itemsAddedToCollection.Clear();

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (!_items.Contains(item))
                return false;

            if (!_itemsAddedToCollection.Remove(item))
                _itemsRemovedFromCollection.Add(item);

            _items.Remove(item);

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

            return true;
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IList<T>

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_items.Contains(item))
                throw new InvalidOperationException("This item is already in the collection.");

            _items.Insert(index, item);
            _itemsAddedToCollection.Add(item);
            _itemsRemovedFromCollection.Remove(item);

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void RemoveAt(int index)
        {
            if (index >= _items.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            T item = _items[index];

            if (!_itemsAddedToCollection.Remove(item))
                _itemsRemovedFromCollection.Add(item);

            _items.RemoveAt(index);

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        public T this[int index]
        {
            get { return _items[index]; }
            set
            {
                if (index > _items.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (index == _items.Count)
                {
                    Insert(index, value);
                }
                else
                {
                    T oldItem = _items[index];

                    if (Equals(oldItem, value)) return;

                    if (!_itemsAddedToCollection.Remove(oldItem))
                        _itemsRemovedFromCollection.Add(oldItem);

                    if (!_itemsRemovedFromCollection.Remove(value))
                        _itemsAddedToCollection.Add(value);

                    _items[index] = value;

                    OnPropertyChanged(IndexerName);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                        value, oldItem, index));
                }
            }
        }

        #endregion

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        #endregion
    }
}
