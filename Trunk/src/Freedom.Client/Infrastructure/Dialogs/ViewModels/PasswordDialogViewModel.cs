using Freedom.Domain.CommandModel;
using Freedom.Domain.CommandModel.Users;
using Freedom.Domain.Exceptions;
using Freedom.Domain.Services.Command;
using Freedom.Domain.Services.Security;
using Freedom.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.Client.Infrastructure.Dialogs.ViewModels
{
    internal class PasswordDialogViewModel : DialogViewModel
    {
        private readonly ICommandService _commandService = IoC.Get<ICommandService>();

        private string _currentPassword;
        private string _newPassword1;
        private string _newPassword2;
        private string _errorText;
        private int _passwordLength;
        private int _passwordComplexity;
        private bool _forcePasswordChange = true;

        public PasswordDialogViewModel()
        {
            DefaultButtonCaption = "_Change Password";
        }

        public virtual string MainInstructionText => "Change password for " + UserName;

        public virtual string UserName
            => (App.User as FreedomPrincipal)?.DisplayName ?? App.User?.Identity?.Name ?? "Current User";

        public virtual bool CurrentPasswordRequired => true;

        public string CurrentPassword
        {
            get { return _currentPassword; }
            set
            {
                if (_currentPassword == value) return;
                _currentPassword = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNewPasswordDifferent));
            }
        }

        public string NewPassword1
        {
            get { return _newPassword1; }
            set
            {
                if (_newPassword1 == value) return;
                _newPassword1 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DoPasswordsMatch));
                OnPropertyChanged(nameof(IsNewPasswordDifferent));
                OnPropertyChanged(nameof(IsNewPasswordValid));
            }
        }

        public string NewPassword2
        {
            get { return _newPassword2; }
            set
            {
                if (_newPassword2 == value) return;
                _newPassword2 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DoPasswordsMatch));
                OnPropertyChanged(nameof(IsNewPasswordValid));
            }
        }

        public bool ForcePasswordChange
        {
            get { return _forcePasswordChange; }
            set
            {
                if (value == _forcePasswordChange) return;
                _forcePasswordChange = value;
                OnPropertyChanged();
            }
        }

        public virtual bool ShowForcePasswordChange => false;

        public int PasswordLength
        {
            get { return _passwordLength; }
            set
            {
                if (_passwordLength == value) return;
                _passwordLength = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNewPasswordLongEnough));
            }
        }

        public int PasswordComplexity
        {
            get { return _passwordComplexity; }
            set
            {
                if (_passwordComplexity == value) return;
                _passwordComplexity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNewPasswordComplexEnough));
            }
        }

        public bool DoPasswordsMatch => !string.IsNullOrEmpty(_newPassword1) && _newPassword1 == _newPassword2;

        public bool IsNewPasswordDifferent
        {
            get
            {
                if (!CurrentPasswordRequired)
                    return false;

                return !string.IsNullOrEmpty(CurrentPassword) &&
                       !string.IsNullOrEmpty(NewPassword1) &&
                       CurrentPassword != NewPassword1;
            }
        }

        public bool IsNewPasswordLongEnough => _passwordLength >= MinimumPasswordLength;

        public bool IsNewPasswordComplexEnough => _passwordComplexity >= MinimumPasswordComplexity;

        public virtual bool IsNewPasswordValid => DoPasswordsMatch && IsNewPasswordLongEnough && IsNewPasswordComplexEnough && IsNewPasswordDifferent;

        public virtual int MinimumPasswordLength => ApplicationSettings.Current.MinimumPasswordLength;

        public virtual int MinimumPasswordComplexity => ApplicationSettings.Current.MinimumPasswordComplexity;

        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                if (_errorText == value) return;
                _errorText = value;
                OnPropertyChanged();
            }
        }

        public override IEnumerable<DialogButtonViewModelBase> DialogButtons
        {
            get
            {
                yield return new AsyncDialogButtonViewModel(DefaultButtonCaption, ChangePasswordAsync, () => IsNewPasswordValid, DialogButtonOptions.IsDefault);
                yield return new DialogButtonViewModel(CancelButtonCaption, () => DialogResult = false, () => CanClickCancelButton, DialogButtonOptions.IsCancel);
            }
        }

        private async Task ChangePasswordAsync()
        {
            ErrorText = null;

            if (string.IsNullOrEmpty(NewPassword1))
            {
                ErrorText = "The new password can not be left blank.";
                return;
            }

            using (new WindowWaitMonitor(this, "Changing Password"))
            {
                try
                {
                    bool result = await InternalChangePasswordAsync();

                    if (result == false)
                    {
                        ErrorText = "Your current password is incorrect.";
                        return;
                    }
                }
                catch (HttpStatusCommunicationException ex)
                {
                    if (ex.StatusCode != HttpStatusCode.Unauthorized && ex.StatusCode != HttpStatusCode.Forbidden)
                        throw;

                    ErrorText = "Your current password is incorrect.";
                    return;
                }

                DialogResult = true;
            }
        }

        internal virtual async Task<bool> InternalChangePasswordAsync()
        {
            ChangePasswordCommand command = new ChangePasswordCommand();

            command.CurrentPassword = CurrentPassword;
            command.NewPassword = NewPassword1;

            CommandResult result = await _commandService.ExecuteAsync(command);

            if (!result.Success)
                return false;

            IIdentity newIdentity = new FreedomCredentials(App.User.Identity.Name, NewPassword1);

            App.User = new FreedomPrincipal(newIdentity, (FreedomPrincipal)App.User);

            return true;
        }
    }
}
