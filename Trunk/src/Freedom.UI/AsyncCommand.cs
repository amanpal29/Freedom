using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Freedom.UI
{
    [Flags]
    public enum AsyncCommandOptions
    {
        None = 0x0,
        AllowMutiple = 0x1
    }

    public class AsyncCommand : IAsyncCommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Predicate<object> _canExecute;
        private readonly AsyncCommandOptions _options;
        private bool _isExecuting;

        public AsyncCommand(Func<Task> execute, AsyncCommandOptions options = AsyncCommandOptions.None)
            : this(dummy => execute(), null, options)
        {

        }

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute, AsyncCommandOptions options = AsyncCommandOptions.None)
            : this(dummy => execute(), dummy => canExecute(), options)
        {

        }

        public AsyncCommand(Func<object, Task> execute, Predicate<object> canExecute = null,
            AsyncCommandOptions options = AsyncCommandOptions.None)
        {
            _execute = execute;
            _canExecute = canExecute;
            _options = options;
        }

        public bool CanExecute(object parameter)
        {
            if (_isExecuting)
                return false;

            return _canExecute == null || _canExecute(parameter);
        }

        public async void Execute(object parameter)
        {
            if (_options.HasFlag(AsyncCommandOptions.AllowMutiple))
            {
                await ExecuteAsync(parameter);
            }
            else
            {
                if (_isExecuting) return;

                try
                {
                    _isExecuting = true;
                    RaiseCanExecuteChanged();
                    await ExecuteAsync(parameter);
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public Task ExecuteAsync(object parameter)
        {
            return _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
