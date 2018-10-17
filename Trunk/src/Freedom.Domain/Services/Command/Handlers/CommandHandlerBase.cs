using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;
using Freedom.Domain.Model;

namespace Freedom.Domain.Services.Command.Handlers
{
    public abstract class CommandHandlerBase<TCommand> : ICommandHandler
        where TCommand : CommandBase
    {
        public virtual bool CanHandle(CommandBase command)
        {
            return command is TCommand;
        }

        public abstract IEnumerable<SystemPermission> RequiredPermissions { get; }

        public virtual bool MustBeAdministrator => false;

        public abstract Task<CommandResult> Handle(CommandBase command, CommandExecutionContext context);

        public virtual bool IsAuthorized(CommandBase command, CommandExecutionContext context)
        {
            if (MustBeAdministrator && !context.IsAdministrator)
                return false;

            return RequiredPermissions.All(context.Permissions.Contains);
        }
    }
}
