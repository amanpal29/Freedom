using System.Threading.Tasks;
using Freedom.Domain.CommandModel;
using Freedom.Domain.Services.Command;

namespace Freedom.Domain.Infrastructure
{
    /// <summary>
    /// A INotificationFactory that does nothing.
    /// </summary>
    public class NullNotificationFactory : INotificationFactory
    {
        public Task<bool> TryCreateCommandFailedNotificationAsync(CommandBase command, CommandExecutionContext context, string title,
            string description)
        {
            return Task.FromResult(false);
        }
    }
}
