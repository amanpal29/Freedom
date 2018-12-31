using Freedom.Domain.CommandModel;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Domain.Services.Command;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Client.Services.Command
{
    internal class CommandServiceManualRetryDecorator : ICommandService
    {
        private readonly ICommandService _commandService;

        public CommandServiceManualRetryDecorator(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public Task<CommandResult> ExecuteAsync(CommandBase command, CancellationToken cancellationToken)
        {
            return IoC.Get<IExceptionHandlerService>().RetryAsync(ct =>
                _commandService.ExecuteAsync(command, ct),
                cancellationToken);
        }
    }
}