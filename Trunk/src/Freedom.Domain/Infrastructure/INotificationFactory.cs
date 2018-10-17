using System;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;
using Freedom.Domain.Services.Command;

namespace Freedom.Domain.Infrastructure
{
    public interface INotificationFactory
    {
        Task<bool> TryCreateCommandFailedNotificationAsync(CommandBase command, CommandExecutionContext context, string title, string description);
    }

    public static class NotificationFactoryExtenstions
    {
        public static Task<bool> TryCreateCommandFailedNotificationAsync(this INotificationFactory notificationFactory, CommandBase command, CommandExecutionContext context, Exception exception)
        {
            return notificationFactory.TryCreateCommandFailedNotificationAsync(
                command,
                context,
                $"{command.DisplayName} Failed Unexpectedly",
                "An unexpected exception occurred when executing the command. Please contact your Freedom Administrator. " + exception.Message);
        }
    }
}