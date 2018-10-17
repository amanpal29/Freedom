using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;
using Freedom.Domain.CommandModel.Users;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Command;
using Freedom.Domain.Services.Command.Handlers;
using log4net;

namespace Freedom.WebApi.Services.Command.Handlers.Users
{
    public class ResetPasswordCommandHandler : CommandHandlerBase<ResetPasswordCommand>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool MustBeAdministrator => true;

        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield break; }
        }

        public override async Task<CommandResult> Handle(CommandBase commandBase, CommandExecutionContext context)
        {
            ResetPasswordCommand command = (ResetPasswordCommand) commandBase;

            if (string.IsNullOrEmpty(command.NewPassword))
                return new CommandResult(false);

            Log.Info($"Resetting the password for Service Provider with id {command.UserId}.");

            using (DbConnection connection = IoC.Get<DbConnection>())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                if (!await PasswordCommon.ChangePasswordAsync(connection, command.UserId, command.NewPassword, command.ForcePasswordChange))
                    return new CommandResult(false);

                Log.Info($"The password for Service Provider with id {command.UserId} was successfully changed.");

                return new CommandResult(true);
            }
        }
    }
}