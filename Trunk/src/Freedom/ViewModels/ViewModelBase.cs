using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Freedom.Annotations;

namespace Freedom.ViewModels
{
    [Serializable]
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Logical ViewModel Children

        [Browsable(false)]
        [XmlIgnore]
        public virtual IEnumerable<ViewModelBase> Children => null;

        #endregion

        #region Model to ViewModel to View Notifications

        [NonSerialized]
        private Dictionary<INotifyPropertyChanged, NameValueCollection> _modelNotifications;

        public void AddModelNotifications(INotifyPropertyChanged model)
        {
            if (model == null) return;

            IEnumerable<string> modelProperties = TypeDescriptor.GetProperties(model).OfType<PropertyDescriptor>().Select(p => p.Name);
            IEnumerable<string> viewModelProperties = TypeDescriptor.GetProperties(this).OfType<PropertyDescriptor>().Select(p => p.Name);

            foreach (string commonProperty in modelProperties.Intersect(viewModelProperties))
                AddModelNotification(model, commonProperty, commonProperty);
        }

        public void AddModelNotification(INotifyPropertyChanged model, string propertyName)
        {
            if (model == null) return;

            AddModelNotification(model, propertyName, propertyName);
        }

        public void AddModelNotification(INotifyPropertyChanged model, string modelPropertyName, string viewModelPropertyName)
        {
            if (model == null) return;

            if (_modelNotifications == null)
            {
                _modelNotifications = new Dictionary<INotifyPropertyChanged, NameValueCollection>();
            }

            NameValueCollection propertyNameCrossReference;

            if (_modelNotifications.ContainsKey(model))
            {
                propertyNameCrossReference = _modelNotifications[model];
            }
            else
            {
                propertyNameCrossReference = new NameValueCollection();

                _modelNotifications.Add(model, propertyNameCrossReference);

                model.PropertyChanged += OnModelPropertyChanged;
            }

            VerifyPropertyName(model, modelPropertyName);
            VerifyPropertyName(this, viewModelPropertyName);

            propertyNameCrossReference.Add(modelPropertyName, viewModelPropertyName);
        }

        public void RemoveModelNotifications(INotifyPropertyChanged model)
        {
            if (model == null || _modelNotifications == null) return;
            model.PropertyChanged -= OnModelPropertyChanged;
            _modelNotifications.Remove(model);
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            INotifyPropertyChanged notifyPropertyChanged = sender as INotifyPropertyChanged;

            if (notifyPropertyChanged == null || !_modelNotifications.ContainsKey(notifyPropertyChanged)) return;

            NameValueCollection propertyNameCrossReference = _modelNotifications[notifyPropertyChanged];

            string[] viewModelPropertyNames = propertyNameCrossReference.GetValues(e.PropertyName);

            if (viewModelPropertyNames == null || viewModelPropertyNames.Length <= 0) return;

            foreach (string viewModelPropertyName in viewModelPropertyNames)
            {
                OnPropertyChanged(viewModelPropertyName);
            }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            VerifyPropertyName(this, args.PropertyName);

            PropertyChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Warns the developer if this object does not have a public property with the specified name.
        /// This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private static void VerifyPropertyName(object obj, string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && propertyName != "Item[]" &&
                TypeDescriptor.GetProperties(obj)[propertyName] == null)
            {
                throw new ArgumentException("Invalid property name: " + propertyName, nameof(propertyName));
            }
        }

        #endregion
    }
}