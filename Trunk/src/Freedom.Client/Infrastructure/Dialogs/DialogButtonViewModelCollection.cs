using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using Freedom.Annotations;

namespace Freedom.Client.Infrastructure.Dialogs
{
    public class DialogButtonViewModelCollection : IList<DialogButtonViewModelBase>, INotifyPropertyChanged
    {
        private readonly List<DialogButtonViewModelBase> _buttons;
        private int _asyncExecutionsInProgress;

        public DialogButtonViewModelCollection()
        {
            _buttons = new List<DialogButtonViewModelBase>();
        }

        public DialogButtonViewModelCollection(IEnumerable<DialogButtonViewModelBase> buttons)
        {
            _buttons = new List<DialogButtonViewModelBase>(buttons);
        }

        public bool IsExecuting => _asyncExecutionsInProgress > 0;

        public void IncrementAsyncOperationCount()
        {
            if (Interlocked.Increment(ref _asyncExecutionsInProgress) != 1) return;
            OnPropertyChanged(nameof(IsExecuting));
            CommandManager.InvalidateRequerySuggested();
        }

        public void DecrementAsyncOperationCount()
        {
            if (Interlocked.Decrement(ref _asyncExecutionsInProgress) != 0) return;
            OnPropertyChanged(nameof(IsExecuting));
            CommandManager.InvalidateRequerySuggested();
        }

        #region Implementation of IEnumerable

        public IEnumerator<DialogButtonViewModelBase> GetEnumerator()
        {
            return _buttons.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _buttons.GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<DialogButtonViewModelBase>

        public void Add(DialogButtonViewModelBase item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _buttons.Add(item);

            item.Owner = this;
        }

        public void Clear()
        {
            _buttons.Clear();
        }

        public bool Contains(DialogButtonViewModelBase item)
        {
            return _buttons.Contains(item);
        }

        public void CopyTo(DialogButtonViewModelBase[] array, int arrayIndex)
        {
            _buttons.CopyTo(array, arrayIndex);
        }

        public bool Remove(DialogButtonViewModelBase item)
        {
            return _buttons.Remove(item);
        }

        public int Count => _buttons.Count;

        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IList<DialogButtonViewModelBase>

        public int IndexOf(DialogButtonViewModelBase item)
        {
            return _buttons.IndexOf(item);
        }

        public void Insert(int index, DialogButtonViewModelBase item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _buttons.Insert(index, item);

            item.Owner = this;
        }

        public void RemoveAt(int index)
        {
            _buttons.RemoveAt(index);
        }

        public DialogButtonViewModelBase this[int index]
        {
            get { return _buttons[index]; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _buttons[index] = value;

                value.Owner = this;
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
    }
}
