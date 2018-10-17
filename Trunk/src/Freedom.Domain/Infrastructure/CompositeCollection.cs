using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using Freedom.Domain.Interfaces;
using Freedom.Domain.Model;

namespace Freedom.Domain.Infrastructure
{
    public class CompositeCollection<TIntermediate, TChild> : IEntityCollection<TChild>, INotifyCollectionChanged
        where TIntermediate : new()
        where TChild : EntityBase
    {
        #region Fields

        private readonly ICollection<TIntermediate> _intermediateCollection;

        private readonly Func<TIntermediate, TChild> _getChild;
        private readonly Func<TIntermediate, Guid> _getKey;
        private readonly Action<TIntermediate, TChild> _setChild;
        private readonly Action<TIntermediate, Guid> _setKey;

        #endregion

        #region Constructor

        public CompositeCollection(ICollection<TIntermediate> intermediateCollection,
            Expression<Func<TIntermediate, TChild>> getChild,
            Expression<Func<TIntermediate, Guid>> getKey)
        {
            if (intermediateCollection == null)
                throw new ArgumentNullException(nameof(intermediateCollection));

            if (getChild == null)
                throw new ArgumentNullException(nameof(getChild));

            if (getKey == null)
                throw new ArgumentNullException(nameof(getKey));

            _intermediateCollection = intermediateCollection;

            INotifyCollectionChanged notifyCollectionChanged = intermediateCollection as INotifyCollectionChanged;

            if (notifyCollectionChanged != null)
            {
                notifyCollectionChanged.CollectionChanged += IntermediateCollection_CollectionChanged;
            }

            _getChild = getChild.Compile();

            _getKey = getKey.Compile();

            _setChild = DelegateFactory.PropertySetAction(getChild);

            _setKey = DelegateFactory.PropertySetAction(getKey);
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<TChild> GetEnumerator()
        {
            return _intermediateCollection.Select(_getChild).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<TChild>

        public void Add(TChild item)
        {
            if (item == null || Contains(item.Id)) return;

            TIntermediate intermediate = new TIntermediate();

            _setChild(intermediate, item);

            _setKey(intermediate, item.Id);

            _intermediateCollection.Add(intermediate);
        }

        public void Clear()
        {
            _intermediateCollection.Clear();
        }

        public bool Contains(TChild item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return Contains(item.Id);
        }

        public void CopyTo(TChild[] array, int arrayIndex)
        {
            foreach (TChild child in _intermediateCollection.Select(_getChild))
                array[arrayIndex++] = child;
        }

        public bool Remove(TChild item)
        {
            if (item == null)
                return false;

            return Remove(item.Id);
        }

        public int Count => _intermediateCollection.Count;

        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IEntityCollection<TChild>

        public void Add(Guid id)
        {
            if (!Contains(id))
            {
                TIntermediate intermediate = new TIntermediate();

                _setKey(intermediate, id);

                _intermediateCollection.Add(intermediate);
            }
        }

        public bool Remove(Guid id)
        {
            List<TIntermediate> itemsToRemove = _intermediateCollection.Where(i => _getKey(i) == id).ToList();

            foreach (TIntermediate item in itemsToRemove)
                _intermediateCollection.Remove(item);

            return itemsToRemove.Count > 0;
        }

        public bool Contains(Guid id)
        {
            return _intermediateCollection.Any(i => _getKey(i) == id);
        }

        public IEnumerable<Guid> Keys => _intermediateCollection.Select(_getKey);

        #endregion

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        private void IntermediateCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs args;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    args = new NotifyCollectionChangedEventArgs(e.Action, BuildChildArray(e.NewItems),
                        e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Move:
                    args = new NotifyCollectionChangedEventArgs(e.Action, BuildChildArray(e.NewItems),
                        e.NewStartingIndex, e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    args = new NotifyCollectionChangedEventArgs(e.Action, BuildChildArray(e.OldItems),
                        e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    args = new NotifyCollectionChangedEventArgs(e.Action, BuildChildArray(e.NewItems),
                        e.NewStartingIndex);
                    break;

                default:
                    args = new NotifyCollectionChangedEventArgs(e.Action);
                    break;
            }

            OnCollectionChanged(args);
        }

        private IList BuildChildArray(IList items)
        {
            if (items == null)
                return null;

            TChild[] result = new TChild[items.Count];

            for (int i = 0; i < items.Count; i++)
                result[i] = items[i] is TIntermediate ? _getChild((TIntermediate) items[i]) : null;

            return result;
        }

        #endregion
    }
}