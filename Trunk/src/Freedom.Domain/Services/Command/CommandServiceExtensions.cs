using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;

namespace Freedom.Domain.Services.Command
{
    public static class CommandServiceExtensions
    {
        public static Task<CommandResult> ExecuteAsync(this ICommandService commandService, CommandBase command)
        {
            return commandService.ExecuteAsync(command, CancellationToken.None);
        }
    }
}