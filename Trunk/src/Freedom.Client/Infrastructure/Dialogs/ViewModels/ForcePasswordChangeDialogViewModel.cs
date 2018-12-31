using Freedom.Domain.CommandModel;
using Freedom.Domain.CommandModel.Users;
using Freedom.Domain.Exceptions;
using Freedom.Domain.Services.Command;
using Freedom.Domain.Services.Security;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Freedom.Client.Infrastructure.Dialogs.ViewModels
{
    internal class ForcePasswordChangeDialogViewModel : PasswordDialogViewModel
    {
        private readonly ICommandService _commandService = IoC.Get<ICommandService>();

        private readonly PasswordExpiredException _passwordExpiredException;

        public ForcePasswordChangeDialogViewModel(string userName, PasswordExpiredException passwordExpiredException)
        {
            UserName = userName;

            _passwordExpiredException = passwordExpiredException;
        }

        public override string MainInstructionText => "You must change your password to continue.";

        public override string UserName { get; }

        public override int MinimumPasswordLength => _passwordExpiredException.MinimumPasswordLength;

        public override int MinimumPasswordComplexity => _passwordExpiredException.MinimumPasswordComplexity;

        #region Overrides of PasswordDialogViewModel

        internal override async Task<bool> InternalChangePasswordAsync()
        {
            ChangePasswordCommand command = new ChangePasswordCommand();

            command.CurrentPassword = CurrentPassword;
            command.NewPassword = NewPassword1;

            App.User = new GenericPrincipal(new FreedomCredentials(UserName, command.CurrentPassword), new string[0]);

            CommandResult result = await _commandService.ExecuteAsync(command);

            App.User = null;

            return result.Success;
        }

        #endregion
    }
}
