using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Freedom.Domain.CommandModel;
using Freedom.WebApi.Filters;
using Freedom.Domain.Infrastructure;
using Freedom.Domain.Services.Command;
using Freedom.Domain.Services.Security;
using Freedom.Domain.Services.Time;
using log4net;

namespace Freedom.WebApi.Controllers
{
    [FreedomAuthentication]
    public class CommandController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICommandHandlerCollection _commandHandlers = IoC.Get<ICommandHandlerCollection>();
        private readonly INotificationFactory _notificationFactory = IoC.TryGet<INotificationFactory>();
        private readonly ITimeService _timeService = IoC.TryGet<ITimeService>() ?? new LocalTimeService();

        [HttpPost]
        [Route("command/execute")]
        public virtual async Task<IHttpActionResult> ExecuteAsync([FromBody] CommandBase command, CancellationToken cancellationToken)
        {
            CommandExecutionContext context = new CommandExecutionContext((FreedomPrincipal) User, _timeService.UtcNow, CommandExecutionContextOptions.None);

            try
            {
                ICommandHandler handler = _commandHandlers.FindHandlerForCommand(command);

                if (handler == null)
                {
                    Log.Warn($"The command {command.GetType().Name} is not supported on this server. Request will be ignored.");

                    return StatusCode(HttpStatusCode.NotImplemented);
                }

                if (!context.IsAdministrator && !handler.IsAuthorized(command, context))
                {
                    Log.Warn($"The user {User?.Identity?.Name} is not authorized to execute command {command.GetType().Name}. Request will be ignored.");

                    return StatusCode(HttpStatusCode.Forbidden);
                }

                return Ok(await handler.Handle(command, context));
            }
            catch (Exception ex)
            {
                if (_notificationFactory != null && command.ExecuteNonInteractive)
                {
                    Log.Info("Creating a notification to the user of their failed command.", ex);

                    await _notificationFactory.TryCreateCommandFailedNotificationAsync(command, context, ex);

                    return Ok(new CommandResult(false));
                }

                throw;
            }
        }
    }
}