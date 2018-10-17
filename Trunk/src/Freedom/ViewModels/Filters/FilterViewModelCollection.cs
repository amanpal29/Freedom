using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Freedom.Annotations;
using Freedom.Constraints;
using Freedom.Linq;

namespace Freedom.ViewModels.Filters
{
    public class FilterViewModelCollection : IList<FilterViewModel>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region Fields

        private bool _loadingMemento;
        private readonly List<FilterViewModel> _filters = new List<FilterViewModel>();

        #endregion

        public Constraint Constraint
        {
            get
            {
                AndConstraint result = new AndConstraint();

                foreach (FilterViewModel filterViewModel in _filters)
                {
                    Constraint constraint = filterViewModel.Constraint;

                    if (constraint is AndConstraint)
                        result.AddRange((IEnumerable<Constraint>) constraint);
                    else if (constraint != null)
                        result.Add(constraint);
                }

                return result.Count == 1 ? result[0] : result;
            }
        }

        protected virtual void HandleConstraintChanged(object sender, EventArgs e)
        {
            if (!_loadingMemento)
                OnConstraintChanged();
        }

        protected virtual void OnConstraintChanged()
        {
            OnPropertyChanged(nameof(Constraint));
        }

        #region Filter Memento

        public string GetMemento()
        {
            List<FilterMementoPair> mementoData = new List<FilterMementoPair>();

            foreach (FilterViewModel filterViewModel in _filters)
                if (!string.IsNullOrEmpty(filterViewModel.Name))
                    mementoData.Add(new FilterMementoPair(filterViewModel.Name, filterViewModel.SelectedValue));

            return XmlSerializationHelper.Serialize(mementoData);
        }

        public virtual void LoadFromMemento(string memento)
        {
            if (string.IsNullOrEmpty(memento)) return;

            List<FilterMementoPair> mementoData = XmlSerializationHelper.Deserialize<List<FilterMementoPair>>(memento);

            try
            {
                _loadingMemento = true;

                foreach (FilterMementoPair pair in mementoData)
                {
                    string key = pair.Key;

                    foreach (FilterViewModel filter in _filters.Where(f => f.Name == key))
                        filter.SelectedValue = pair.Value;
                }
            }
            finally
            {
                _loadingMemento = false;
            }

            OnConstraintChanged();
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<FilterViewModel> GetEnumerator()
        {
            return _filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<FilterViewModel>

        public void Add(FilterViewModel item)
        {
            if (item == null) return;

            _filters.Add(item);

            item.ConstraintChanged += HandleConstraintChanged;

            OnPropertyChanged(nameof(Count));

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item, _filters.Count - 1));
        }



        public void Clear()
        {
            foreach (FilterViewModel item in _filters)
                item.ConstraintChanged -= HandleConstraintChanged;

            _filters.Clear();

            OnPropertyChanged(nameof(Count));

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(FilterViewModel item)
        {
            return _filters.Contains(item);
        }

        public void CopyTo(FilterViewModel[] array, int arrayIndex)
        {
            _filters.CopyTo(array, arrayIndex);
        }

        public bool Remove(FilterViewModel item)
        {
            if (!_filters.Remove(item))
                return false;

            if (item != null)
                item.ConstraintChanged -= HandleConstraintChanged;

            OnPropertyChanged(nameof(Count));

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

            return true;
        }

        public int Count => _filters.Count;

        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IList<FilterViewModel>

        public int IndexOf(FilterViewModel item)
        {
            return _filters.IndexOf(item);
        }

        public void Insert(int index, FilterViewModel item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _filters.Insert(index, item);

            item.ConstraintChanged += HandleConstraintChanged;

            OnPropertyChanged(nameof(Count));

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void RemoveAt(int index)
        {
            FilterViewModel item = _filters[index];

            item.ConstraintChanged -= HandleConstraintChanged;

            _filters.RemoveAt(index);

            OnPropertyChanged(nameof(Count));

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        public FilterViewModel this[int index]
        {
            get { return _filters[index]; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                FilterViewModel item = _filters[index];

                if (item == value) return;

                item.ConstraintChanged -= HandleConstraintChanged;
                
                _filters[index] = value;

                value.ConstraintChanged += HandleConstraintChanged;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, item));
            }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        #endregion
    }

    public class FilterViewModelCollection<T> : FilterViewModelCollection, IFilterViewModel<T>
    {
        public bool IsPredicateSupported
        {
            get { return this.All(x => x is IFilterViewModel<T>); }
        }

        public Func<T, bool> Predicate
        {
            get
            {
                if (!IsPredicateSupported)
                    throw new InvalidOperationException("At least one of the FilterViewModels in the collection doesn't support predicates.");

                switch (Count)
                {
                    case 0:
                        return x => true;

                    case 1:
                        return ((IFilterViewModel<T>) this[0]).Predicate;

                    default:
                        return MatchesAllPredicates;
                }
            }
        }

        public Expression<Func<T, bool>> Expression
        {
            get
            {
                if (!IsPredicateSupported)
                    throw new InvalidOperationException("At least one of the FilterViewModels in the collection doesn't support predicates.");

                switch (Count)
                {
                    case 0:
                        return x => true;

                    case 1:
                        return ((IFilterViewModel<T>) this[0]).Expression;

                    default:
                        return ExpressionHelper.All(this
                                .Cast<MutuallyExclusiveFilterViewModel<T>>()
                                .Select(vm => vm.Expression));
                }
            }
        }

        public bool MatchesAllPredicates(T item)
        {
            if (item == null)
                return false;

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (MutuallyExclusiveFilterViewModel<T> filter in this)
                if (!filter.Predicate(item))
                    return false;

            return true;
        }

        protected override void OnConstraintChanged()
        {
            base.OnConstraintChanged();
            OnPropertyChanged(nameof(Predicate));
            OnPropertyChanged(nameof(Expression));
        }
    }
}
