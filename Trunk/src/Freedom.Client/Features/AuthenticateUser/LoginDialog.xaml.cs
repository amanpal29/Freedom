using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Freedom.Client.Features.AuthenticateUser
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        public LoginDialog(LoginViewModel loginViewModel)
        {
            DataContext = loginViewModel;

            InitializeComponent();
        }

        public Task<bool> ShowAsync()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            LoginViewModel loginViewModel = (LoginViewModel)DataContext;

            loginViewModel.PropertyChanged += (s, a) =>
            {
                if (a.PropertyName == nameof(LoginViewModel.LoginResult) && loginViewModel.LoginResult != null)
                    taskCompletionSource.SetResult(loginViewModel.LoginResult.Value);
            };

            Show();

            return taskCompletionSource.Task;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Set the initial focus...
            if (string.IsNullOrEmpty(UserNameTextBox.Text))
                UserNameTextBox.Focus();
            else
                PasswordBox.Child.Focus();
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            // Sets the Caps Lock indicator status, in case it changed while another window had focus
            CapsLockWarningStackPanel.Visibility = Console.CapsLock ? Visibility.Visible : Visibility.Hidden;
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.CapsLock)
            {
                CapsLockWarningStackPanel.Visibility = Console.CapsLock ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            LoginViewModel loginViewModel = DataContext as LoginViewModel;

            if (loginViewModel != null && !loginViewModel.LoginResult.HasValue)
                loginViewModel.LoginResult = false;
        }
    }
}
