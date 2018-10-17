using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class FullTextSearchConstraint : Constraint, INotifyPropertyChanged
    {
        private string _searchText;

        public FullTextSearchConstraint()
        {
        }

        public FullTextSearchConstraint(string searchText)
        {
            _searchText = searchText;
        }

        [DataMember]
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText) return;
                _searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        #region Overrides of Constraint

        public override ConstraintType ConstraintType => ConstraintType.FullTextSearch;

        public override bool IsEmpty => string.IsNullOrWhiteSpace(SearchText);

        public override bool IsValid => true;

        public override bool? ReducedValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_searchText))
                    return true;

                return null;
            }
        }

        #endregion

        #region Implemention of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
