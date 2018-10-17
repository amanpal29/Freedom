using Freedom.Domain.Model;

namespace Freedom.Domain.Services.Command
{
    public static class CommandExecutionContextExtensions
    {
        public static Notification CreateNotification(this CommandExecutionContext context, NotificationClass notificationClass)
        {
            Notification notification = new Notification();

            notification.Class = notificationClass;
            notification.RecipientId = context.CurrentUserId;
            notification.CreatedDateTime = context.CurrentDateTime;

            return notification;
        }
    }
}
