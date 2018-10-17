using System.Collections.Generic;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel.Users;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Command;
using Freedom.Domain.Services.Command.Handlers;

namespace Freedom.WebApi.Services.Command.Handlers.Users
{
    public class UpdateUserCommandHandler : RepositoryCommandHandler<UpdateUserCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.EditUser; }
        }

        protected override async Task<bool> Handle(FreedomRepository repository, UpdateUserCommand command, CommandExecutionContext context)
        {
            await repository.UpdateAsync(command.User);

            return true;
        }
    }
}