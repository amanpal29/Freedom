using System.Threading.Tasks;
using Freedom.Domain.CommandModel;

namespace Freedom.Domain.Services.Command
{
    public interface ICommandHandler
    {
        bool CanHandle(CommandBase command);

        Task<CommandResult> Handle(CommandBase command, CommandExecutionContext context);

        bool IsAuthorized(CommandBase command, CommandExecutionContext context);
    }
}
