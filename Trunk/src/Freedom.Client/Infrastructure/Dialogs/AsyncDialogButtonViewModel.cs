using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Freedom.Client.Infrastructure.Dialogs
{
    public class AsyncDialogButtonViewModel : DialogButtonViewModelBase
    {
        private bool _isExecuting;
        private readonly Func<Task> _execute;

        public AsyncDialogButtonViewModel(string buttonText, Func<Task> execute)
            : this(buttonText, execute, null, DialogButtonOptions.None)
        {
            _execute = execute;
        }

        public AsyncDialogButtonViewModel(string buttonText, Func<Task> execute, DialogButtonOptions buttonOptions)
            : this(buttonText, execute, null, buttonOptions)
        {
            _execute = execute;
        }

        public AsyncDialogButtonViewModel(string buttonText, Func<Task> execute, Func<bool> canExecute)
            : this(buttonText, execute, canExecute, DialogButtonOptions.None)
        {
            _execute = execute;
        }

        public AsyncDialogButtonViewModel(string buttonText, Func<Task> execute, Func<bool> canExecute,
            DialogButtonOptions buttonOptions)
            : base(buttonText, canExecute, buttonOptions)
        {
            _execute = execute;
        }

        public override bool IsAsync => true;

        public override bool CanExecute(object parameter)
        {
            return !_isExecuting && (Owner == null || !Owner.IsExecuting) && base.CanExecute(parameter);
        }

        public override async void Execute(object parameter)
        {
            if (_isExecuting || _execute == null) return;

            try
            {
                _isExecuting = true;

                if (!IsNonBlocking)
                    Owner?.IncrementAsyncOperationCount();
                else
                    CommandManager.InvalidateRequerySuggested();

                await _execute();
            }
            finally
            {
                _isExecuting = false;

                if (!IsNonBlocking)
                    Owner?.DecrementAsyncOperationCount();
                else
                    CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
