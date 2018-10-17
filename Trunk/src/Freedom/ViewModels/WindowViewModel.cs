using System;
using System.ComponentModel;
using System.Threading;

namespace Freedom.ViewModels
{
    public class WindowViewModel : ViewModelBase
    {
        private bool _isOpen = true;
        private int _asyncOperationsInProgress;
        private string _currentBlockingAction;

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen == value) return;
                _isOpen = value;
                OnPropertyChanged();
            }
        }

        public bool IsAwaiting => _asyncOperationsInProgress > 0;

        public bool IsEnabled => !IsWaiting;

        public bool IsWaiting => !string.IsNullOrEmpty(_currentBlockingAction);

        public string CurrentBlockingAction
        {
            get { return _currentBlockingAction; }
            set
            {
                if (value == _currentBlockingAction) return;
                _currentBlockingAction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(IsWaiting));
            }
        }

        public void IncrementAsyncOperationCount()
        {
            if (Interlocked.Increment(ref _asyncOperationsInProgress) == 1)
                OnPropertyChanged(nameof(IsAwaiting));
        }

        public void DecrementAsyncOperationCount()
        {
            if (Interlocked.Decrement(ref _asyncOperationsInProgress) == 0)
                OnPropertyChanged(nameof(IsAwaiting));
        }

        public virtual void OnWindowClosing(object sender, CancelEventArgs eventArgs)
        {
        }

        public virtual void OnWindowClosed(object sender, EventArgs eventArgs)
        {
        }
    }
}