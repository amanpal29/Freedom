using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Freedom.Annotations;

namespace Freedom.Collections
{
    public class ObservableHashSet<T> : ICollection<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        private readonly HashSet<T> _set;

        public ObservableHashSet()
        {
            _set = new HashSet<T>();
        }

        public ObservableHashSet(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
        }

        public ObservableHashSet(IEnumerable<T> collection)
        {
            _set = new HashSet<T>(collection);
        }

        public ObservableHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(collection, comparer);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        public bool Add(T item)
        {
            if (!_set.Add(item))
                return false;

            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));

            return true;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            List<T> itemsAdded = new List<T>();

            foreach (T item in items)
                if (_set.Add(item))
                    itemsAdded.Add(item);

            if (itemsAdded.Count <= 0) return;

            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemsAdded));
        }

        public bool SetEquals(IEnumerable<T> set)
        {
            return _set.SetEquals(set);
        }

        public void Clear()
        {
            if (_set.Count == 0) return;

            _set.Clear();

            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (!_set.Remove(item))
                return false;

            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

            return true;
        }

        public int Count => _set.Count;

        public bool IsReadOnly => false;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
