using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Freedom.Constraints;

namespace Freedom.ViewModels.Filters
{
    public class MutuallyExclusiveFilterViewModel : FilterViewModel, IList<FilterOptionViewModel>
    {
        #region Fields

        private readonly List<FilterOptionViewModel> _options = new List<FilterOptionViewModel>();
        private FilterOptionViewModel _selectedItem;

        #endregion

        #region Methods

        public void Add(string name, string description, Constraint constraint)
        {
            Add(new FilterOptionViewModel(name, description, constraint));
        }

        public void AddSeperator()
        {
            _options.Add(new SeparatorFilterOptionViewModel());
        }

        public void SetToDefault()
        {
            if (_options.Count > 0)
                SelectedItem = _options[0];
        }

        #endregion

        #region Properties

        public virtual FilterOptionViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Constraint));
                OnConstraintChanged();
            }
        }

        public override Constraint Constraint => SelectedItem?.Constraint;

        #endregion

        #region Overrides of FilterViewModel

        public override string SelectedValue
        {
            get { return _selectedItem?.Name; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                FilterOptionViewModel item = _options.FirstOrDefault(x => x.Name == value);

                if (item != null)
                {
                    SelectedItem = item;
                }
            }
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<FilterOptionViewModel> GetEnumerator()
        {
            return _options.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<FilterOptionViewModel>

        public void Add(FilterOptionViewModel item)
        {
            _options.Add(item);

            // if this is the first item added to the collection, select it by default.
            if (_options.Count == 1 && SelectedItem == null)
                SelectedItem = _options[0];
        }

        public void Clear()
        {
            _options.Clear();

            SelectedItem = null;
        }

        public bool Contains(FilterOptionViewModel item)
        {
            return _options.Contains(item);
        }

        public void CopyTo(FilterOptionViewModel[] array, int arrayIndex)
        {
            _options.CopyTo(array, arrayIndex);
        }

        public bool Remove(FilterOptionViewModel item)
        {
            if (!_options.Remove(item))
                return false;

            // if we just removed the selected item, select the first item by default
            if (SelectedItem == item)
                SelectedItem = _options.Count > 0 ? _options[0] : null;

            return true;
        }

        public int Count => _options.Count;

        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IList<FilterOptionViewModel>

        public int IndexOf(FilterOptionViewModel item)
        {
            return _options.IndexOf(item);
        }

        public void Insert(int index, FilterOptionViewModel item)
        {
            _options.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _options.RemoveAt(index);
        }

        public FilterOptionViewModel this[int index]
        {
            get { return _options[index]; }
            set { _options[index] = value; }
        }

        #endregion
    }

    public class MutuallyExclusiveFilterViewModel<T> : MutuallyExclusiveFilterViewModel, IFilterViewModel<T>
    {
        public void Add(string name, string description, Expression<Func<T, bool>> predicate)
        {
            Add(new FilterOptionViewModel<T>(name, description, predicate));
        }

        public Func<T, bool> Predicate
        {
            get
            {
                FilterOptionViewModel<T> selectedItem = SelectedItem as FilterOptionViewModel<T>;

                return selectedItem != null ? selectedItem.Predicate : x => true;
            }
        }

        public Expression<Func<T, bool>> Expression
        {
            get
            {
                FilterOptionViewModel<T> selectedItem = SelectedItem as FilterOptionViewModel<T>;

                return selectedItem?.Expression;
            }
        }

        #region Overrides of MutuallyExclusiveFilterViewModel

        public override FilterOptionViewModel SelectedItem
        {
            get { return base.SelectedItem; }
            set
            {
                base.SelectedItem = value;
                OnPropertyChanged(nameof(Predicate));
                OnPropertyChanged(nameof(Expression));
            }
        }

        #endregion
    }
}
