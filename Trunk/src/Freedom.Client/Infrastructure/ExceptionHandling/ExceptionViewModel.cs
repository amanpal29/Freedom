using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Freedom.ViewModels;
using Freedom.UI;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Client.Infrastructure.Dialogs;

namespace Freedom.Client.Infrastructure.ExceptionHandling
{
    public class ExceptionViewModel : ViewModelBase
    {
        #region Fields

        private readonly Exception _exception;
        private readonly bool _canRetry;
        private readonly bool _canCancel;
        private ExceptionHandledStatus? _dialogResult;

        #endregion

        #region Constructor

        public ExceptionViewModel(Exception exception, bool canRetry, bool canCancel)
        {
            _exception = exception;
            _canCancel = canCancel;
            _canRetry = canRetry;
        }

        #endregion

        #region Properties

        public Exception Exception
        {
            get { return _exception; }
        }

        public bool CanRetry
        {
            get { return _canRetry; }
        }

        public bool CanCancel
        {
            get { return _canCancel; }
        }

        public ExceptionHandledStatus? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    OnPropertyChanged("DialogResult");
                }
            }
        }

        public string Message
        {
            get
            {
#if DEBUG
                return ExceptionLog;
#else
                return this._exception.Message;
#endif
            }
        }

        public string ExceptionLog
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine(_exception.ToString());
                stringBuilder.AppendLine("---------------------------");
                stringBuilder.AppendLine("Exception Chain:");
                stringBuilder.AppendLine(ExceptionChain);
                stringBuilder.AppendLine("---------------------------");
                stringBuilder.AppendLine("Stack Track:");
                stringBuilder.AppendLine(StackTrace);

                return stringBuilder.ToString();
            }
        }

        public string ExceptionChain
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                for (Exception ex = _exception; ex != null; ex = ex.InnerException)
                {
                    stringBuilder.AppendFormat("{0} in {1}\n", ex.Message, ex.Source);
                }

                return stringBuilder.ToString();
            }
        }

        public string StackTrace
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                for (Exception ex = _exception; ex != null; ex = ex.InnerException)
                {
                    stringBuilder.AppendLine(ex.StackTrace);
                }

                return stringBuilder.ToString();
            }
        }

        #endregion

        #region Commands

        public ICommand IgnoreExceptionCommand
        {
            get { return new RelayCommand(obj => DialogResult = ExceptionHandledStatus.Unhandled); }
        }

        public ICommand CancelExceptionCommand
        {
            get { return new RelayCommand(obj => DialogResult = ExceptionHandledStatus.Cancel); }
        }

        public ICommand RetryExceptionCommand
        {
            get { return new RelayCommand(obj => DialogResult = ExceptionHandledStatus.Retry); }
        }

        public ICommand SaveExceptionLogCommand
        {
            get { return new RelayCommand(SaveExceptionLog); }
        }

        private void SaveExceptionLog(object owner)
        {
            Window window = owner is DependencyObject ? Window.GetWindow((DependencyObject) owner) : null;

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.FileName = "FreedomException";
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (.txt)|*.txt|All Files|*.*";

            if (dialog.ShowDialog(window) == true)
            {
                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(dialog.OpenFile()))
                    {
                        streamWriter.Write(ExceptionLog);
                    }

                    DialogResult = ExceptionHandledStatus.Handled;
                }
                catch (IOException e)
                {
                    string message = string.Format("Unable to save file. {0}", e.Message);

                    Dialog.Show(this, message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        #endregion
    }
}
