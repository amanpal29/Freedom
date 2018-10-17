using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;
using Freedom.Domain.CommandModel.Users;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Command.Handlers;
using Freedom.Domain.Services.Command;
using log4net;

namespace Freedom.WebApi.Services.Command.Handlers.Users
{
    public class ChangePasswordCommandHandler : CommandHandlerBase<ChangePasswordCommand>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield break; }
        }

        public override async Task<CommandResult> Handle(CommandBase commandBase, CommandExecutionContext context)
        {
            ChangePasswordCommand command = (ChangePasswordCommand) commandBase;

            if (string.IsNullOrEmpty(command.CurrentPassword) || string.IsNullOrEmpty(command.NewPassword))
                return new CommandResult(false);

            Log.Info($"Attempting to change password for Service Provider with id {context.CurrentUserId}.");

            using (DbConnection connection = IoC.Get<DbConnection>())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                if (!await PasswordCommon.VerifyPasswordAsync(connection, context.CurrentUserId, command.CurrentPassword))
                    return new CommandResult(false);

                if (!await PasswordCommon.ChangePasswordAsync(connection, context.CurrentUserId, command.NewPassword, false))
                    return new CommandResult(false);

                Log.Info($"The password for Service Provider with id {context.CurrentUserId} was successfully changed.");

                return new CommandResult(true);
            }
        }

    }
}