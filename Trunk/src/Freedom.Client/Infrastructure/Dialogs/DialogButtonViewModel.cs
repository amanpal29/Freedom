using System;

namespace Freedom.Client.Infrastructure.Dialogs
{
    public class DialogButtonViewModel : DialogButtonViewModelBase
    {
        private readonly Action _execute;

        public DialogButtonViewModel(string buttonText, Action execute)
            : this(buttonText, execute, null, DialogButtonOptions.None)
        {
        }

        public DialogButtonViewModel(string buttonText, Action execute, DialogButtonOptions buttonOptions)
            : this(buttonText, execute, null, buttonOptions)
        {
        }

        public DialogButtonViewModel(string buttonText, Action execute, Func<bool> canExecute)
            : this(buttonText, execute, canExecute, DialogButtonOptions.None)
        {
        }

        public DialogButtonViewModel(string buttonText, Action execute, Func<bool> canExecute, DialogButtonOptions buttonOptions)
            : base(buttonText, canExecute, buttonOptions)
        {
            _execute = execute;
        }

        public override bool IsAsync => false;

        public override void Execute(object parameter)
        {
            _execute?.Invoke();
        }
    }
}
