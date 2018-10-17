using System;
using Freedom.Constraints;

namespace Freedom.ViewModels.Filters
{
    public abstract class FilterViewModel : ViewModelBase
    {
        private string _name;
        private string _description;
        private bool _isVisible = true;

        protected FilterViewModel()
        {
        }

        protected FilterViewModel(string name, string description)
        {
            _name = name;
            _description = description;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler ConstraintChanged;

        public abstract string SelectedValue { get; set; }

        public abstract Constraint Constraint { get; }

        protected virtual void OnConstraintChanged()
        {
            ConstraintChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
