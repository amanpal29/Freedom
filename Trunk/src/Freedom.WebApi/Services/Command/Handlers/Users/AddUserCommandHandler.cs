using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;
using Freedom.Domain.CommandModel.Users;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Command.Handlers;
using Freedom.Domain.Services.Command;

namespace Freedom.WebApi.Services.Command.Handlers.Users
{
    public class AddUserCommandHandler : RepositoryCommandHandler<AddUserCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.EditUser; }
        }

        public override async Task<CommandResult> Handle(CommandBase commandBase, CommandExecutionContext context)
        {
            CommandResult result = await base.Handle(commandBase, context);

            if (!result.Success)
                return result;

            AddUserCommand command = (AddUserCommand) commandBase;

            using (DbConnection connection = IoC.Get<DbConnection>())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                bool success = await PasswordCommon.ChangePasswordAsync(connection, 
                    command.User.Id, command.Password, command.User.ForcePasswordChange);

                return new CommandResult(success);
            }
        }

        protected override Task<bool> Handle(FreedomRepository repository, AddUserCommand command, CommandExecutionContext context)
        {
            repository.Add(command.User);

            return Task.FromResult(true);
        }
    }
}
