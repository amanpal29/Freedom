using Freedom.Domain.CommandModel;
using Freedom.Domain.Interfaces;
using Freedom.Domain.Services.Command;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Freedom.Client.Services.Command
{
    public class CommandServiceRefreshDecorator : ICommandService
    {
        private readonly ICommandService _commandService;

        public CommandServiceRefreshDecorator(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public async Task<CommandResult> ExecuteAsync(CommandBase command, CancellationToken cancellationToken)
        {
            CommandResult result = await _commandService.ExecuteAsync(command, cancellationToken);

            IRefreshable application = Application.Current as IRefreshable;

            if (application != null)
                await application.RefreshAsync();

            return result;
        }
    }
}
