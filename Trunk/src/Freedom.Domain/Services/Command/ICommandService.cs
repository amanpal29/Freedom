using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;

namespace Freedom.Domain.Services.Command
{
    public interface ICommandService
    {
        Task<CommandResult> ExecuteAsync(CommandBase command, CancellationToken cancellationToken);
    }
}

