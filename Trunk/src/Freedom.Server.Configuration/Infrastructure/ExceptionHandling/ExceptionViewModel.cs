using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.UI;
using Freedom.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Freedom.Server.Tools.Infrastructure.ExceptionHandling
{
    public class ExceptionViewModel : ViewModelBase
    {
        #region Fields

        private ExceptionHandledStatus? _dialogResult;

        #endregion

        #region Constructor

        public ExceptionViewModel(Exception exception, bool canRetry, bool canCancel)
        {
            Exception = exception;
            CanCancel = canCancel;
            CanRetry = canRetry;
        }

        #endregion

        #region Properties

        public Exception Exception { get; }

        public bool CanRetry { get; }

        public bool CanCancel { get; }

        public ExceptionHandledStatus? DialogResult
        {
            get { return _dialogResult; }
            protected set
            {
                if (_dialogResult == value) return;
                _dialogResult = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get
            {
#if DEBUG
                return ExceptionLog;
#else
                return this.ExceptionChain;
#endif
            }
        }

        public string ExceptionLog
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine(Exception.ToString());
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

                for (Exception ex = Exception; ex != null; ex = ex.InnerException)
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

                for (Exception ex = Exception; ex != null; ex = ex.InnerException)
                {
                    stringBuilder.AppendLine(ex.StackTrace);
                }

                return stringBuilder.ToString();
            }
        }

        #endregion

        #region Commands

        public ICommand IgnoreExceptionCommand => new RelayCommand(obj => DialogResult = ExceptionHandledStatus.Unhandled);

        public ICommand CancelExceptionCommand => new RelayCommand(obj => DialogResult = ExceptionHandledStatus.Cancel);

        public ICommand RetryExceptionCommand => new RelayCommand(obj => DialogResult = ExceptionHandledStatus.Retry);

        public ICommand SaveExceptionLogCommand => new RelayCommand(SaveExceptionLog);

        private void SaveExceptionLog(object owner)
        {
            Window window = owner is DependencyObject ? Window.GetWindow((DependencyObject)owner) : null;

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.FileName = "FreedomException";
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (.txt)|*.txt|All Files|*.*";

            if (dialog.ShowDialog(window) != true)
                return;

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
                string message = $"Unable to save file. {e.Message}";

                string caption = Application.Current.MainWindow != null
                    ? Application.Current.MainWindow.Title
                    : "Unable to save file.";

                MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion
    }
}
