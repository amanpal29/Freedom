using Freedom.Server.Tools.Features;
using Freedom.Server.Tools.Infrastructure;
using Freedom.UI;
using log4net;
using log4net.Repository.Hierarchy;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Freedom.Server.Tools
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private readonly Hierarchy _hierarchy = (Hierarchy)LogManager.GetRepository();

        public LogWindow(IEngine  engine)
        {
            DataContext = engine;
            InitializeComponent();

            CloseButton.Command = new RelayCommand(DoClose, () => engine.HasFinished);

            TextBoxAppender appender = new TextBoxAppender(LogTextBox);

            _hierarchy.Root.AddAppender(appender);

            Loaded += (s, a) => engine.Start();

            Unloaded += (s, a) => _hierarchy.Root.RemoveAppender(appender);

        }

        private void DoClose()
        {
            IEngine engine = (IEngine)DataContext;

            DialogResult = engine.Succeeded;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            IEngine engine = (IEngine)DataContext;

            e.Cancel = !engine.HasFinished;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs args)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.FileName = $"Freedom Server Tools Log {DateTime.Now:yyyy-MM-dd}.txt";
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (.txt)|*.txt|All Files|*.*";

            if (dialog.ShowDialog(this) == true)
            {
                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(dialog.OpenFile()))
                    {
                        streamWriter.Write(LogTextBox.Text);
                    }
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
        }
    }
}
