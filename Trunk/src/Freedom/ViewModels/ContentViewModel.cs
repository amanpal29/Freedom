using System;
using System.Xml.Serialization;

namespace Freedom.ViewModels
{
    [Serializable]
    public class ContentViewModel : ViewModelBase
    {
        [NonSerialized]
        private bool _isReadOnly;
        
        [XmlIgnore]
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (_isReadOnly == value) return;
                _isReadOnly = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsContentEnabled));
            }
        }

        [XmlIgnore]
        public bool IsContentEnabled
        {
            get { return !IsReadOnly; }
            set { IsReadOnly = !value; }
        }
    }
}