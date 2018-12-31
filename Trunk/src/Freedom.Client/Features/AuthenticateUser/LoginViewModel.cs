using Freedom.Client.Infrastructure.Dialogs;
using Freedom.Client.Infrastructure.Dialogs.ViewModels;
using Freedom.Client.Properties;
using Freedom.Domain.Exceptions;
using Freedom.UI;
using Freedom.ViewModels;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Freedom.Client.Features.AuthenticateUser
{
    public abstract class LoginViewModel : ViewModelBase
    {
        private string _authenticationStatusMessage;
        private string _userName;
        private string _password;
        private bool _isEnabled;
        private bool? _loginResult;

        protected LoginViewModel()
        {
            _isEnabled = true;
            _userName = Settings.Default.LastLoggedInUserName;
        }

        #region Properties

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName == value) return;
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool? LoginResult
        {
            get { return _loginResult; }
            set
            {
                if (_loginResult == value) return;
                _loginResult = value;
                OnPropertyChanged();
            }
        }

        public string AuthenticationStatusMessage
        {
            get { return _authenticationStatusMessage; }
            set
            {
                if (_authenticationStatusMessage == value) return;
                _authenticationStatusMessage = value;
                OnPropertyChanged();
            }
        }

        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        #endregion

        #region Commands

        public ICommand AuthenticateCommand => new AsyncCommand(AuthenticateAsync, CanAuthenticate);

        private async Task AuthenticateAsync()
        {
            IsEnabled = false;
            AuthenticationStatusMessage = "Authenticating, please wait...";

            try
            {
                if (await TryLoginAsync(_userName, Password))
                {
                    SaveLastLoginInformation();
                    AuthenticationStatusMessage = "Loading, please wait...";
                    LoginResult = true;
                }
            }
            catch (PasswordExpiredException ex)
            {
                ForcePasswordChangeDialogViewModel passwordDialogViewModel =
                    new ForcePasswordChangeDialogViewModel(_userName, ex);

                if (Dialog.Show(passwordDialogViewModel) == true &&
                    await TryLoginAsync(_userName, passwordDialogViewModel.NewPassword1))
                {
                    SaveLastLoginInformation();
                    AuthenticationStatusMessage = "Loading, please wait...";
                    LoginResult = true;
                }
                else
                {
                    LoginResult = false;
                }
            }
            catch (CommunicationException ex)
            {
                AuthenticationStatusMessage = ex.Message;
            }
            finally
            {
                if (!LoginResult.HasValue)
                    IsEnabled = true;
            }
        }

        private void SaveLastLoginInformation()
        {
            Settings.Default.LastLoggedInUserName = UserName;
            Settings.Default.Save();
        }

        private bool CanAuthenticate()
        {
            return IsEnabled && !string.IsNullOrEmpty(_userName) && !string.IsNullOrEmpty(_password);
        }

        protected abstract Task<bool> TryLoginAsync(string userName, string password);
                
        public ICommand CancelCommand => new RelayCommand(obj => LoginResult = false, obj => IsEnabled);

        #endregion
    }
}
