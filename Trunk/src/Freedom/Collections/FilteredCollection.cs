using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Freedom.Annotations;

namespace Freedom.Collections
{
    public class FilteredCollection<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly ICollection<T> _baseCollection;
        private readonly Func<T, bool> _predicate;
        private readonly Action<T> _onAddingItem;

        public FilteredCollection(ICollection<T> baseCollection, Func<T, bool> predicate)
            : this(baseCollection, predicate, null)
        {
        }

        public FilteredCollection(ICollection<T> baseCollection, Func<T, bool> predicate, Action<T> onAddingItem)
        {
            if (baseCollection == null)
                throw new ArgumentNullException(nameof(baseCollection));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            _baseCollection = baseCollection;
            _predicate = predicate;
            _onAddingItem = onAddingItem;

            INotifyCollectionChanged observableBaseCollection = _baseCollection as INotifyCollectionChanged;
            
            if (observableBaseCollection != null)
            {
                observableBaseCollection.CollectionChanged += BaseCollectionChanged;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _baseCollection.Where(_predicate).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void BaseCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            List<T> newItems, oldItems;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    newItems = args.NewItems?.Cast<T>().Where(_predicate).ToList();
                    if (newItems != null && newItems.Count > 0)
                    {
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, newItems));
                        OnPropertyChanged(nameof(Count));
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    oldItems = args.OldItems?.Cast<T>().Where(_predicate).ToList();
                    if (oldItems != null && oldItems.Count > 0)
                    {
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, oldItems));
                        OnPropertyChanged(nameof(Count));
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action));
                    OnPropertyChanged(nameof(Count));
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Replace:
                    newItems = args.NewItems?.Cast<T>().Where(_predicate).ToList() ?? new List<T>();
                    oldItems = args.OldItems?.Cast<T>().Where(_predicate).ToList() ?? new List<T>();
                    if (newItems.Count > 0 || oldItems.Count > 0)
                    {
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, newItems, oldItems));

                        if (newItems.Count != oldItems.Count)
                            OnPropertyChanged(nameof(Count));
                    }
                    break;
            }
        }

        public void Add(T item)
        {
            _onAddingItem?.Invoke(item);

            if (!_predicate(item))
                throw new ArgumentException("The Item being added does not match the filter predicate");

            _baseCollection.Add(item);

            if (!(_baseCollection is INotifyCollectionChanged))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                OnPropertyChanged(nameof(Count));
            }
        }

        public void Clear()
        {
            List<T> existingItems = new List<T>(_baseCollection.Where(_predicate));

            foreach (T item in existingItems)
                _baseCollection.Remove(item);

            if (!(_baseCollection is INotifyCollectionChanged))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                OnPropertyChanged(nameof(Count));
            }
        }

        public bool Contains(T item)
        {
            return _predicate(item) && _baseCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            foreach (T item in _baseCollection.Where(_predicate))
                array[arrayIndex++] = item;
        }

        public bool Remove(T item)
        {
            if (!_predicate(item))
                return false;

            if (!_baseCollection.Remove(item))
                return false;

            if (!(_baseCollection is INotifyCollectionChanged))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                OnPropertyChanged(nameof(Count));
            }

            return true;
        }

        public int Count => _baseCollection.Count(_predicate);

        public bool IsReadOnly => _baseCollection.IsReadOnly;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
