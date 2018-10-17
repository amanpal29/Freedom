using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Freedom.Domain.CommandModel;
using Freedom.Domain.Infrastructure;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Command;
using log4net;

namespace Freedom.WebApi.Infrastructure
{
    public class NotificationFactory : INotificationFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<bool> TryCreateCommandFailedNotificationAsync(CommandBase command, CommandExecutionContext executionContext, string title, string description)
        {
            try
            {
                using (FreedomLocalContext db = IoC.Get<FreedomLocalContext>())
                {
                    Notification notification = new Notification();

                    notification.Class = NotificationClass.CommandFailed;
                    notification.CreatedDateTime = executionContext.CurrentDateTime;
                    notification.RecipientId = executionContext.CurrentUserId;
                    notification.Title = title;
                    notification.Description = description;
                    notification.Payload = SerializeCommand(command);

                    db.Notification.Add(notification);

                    await db.SaveChangesAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Unable to create notification of failed command.", ex);

                return false;
            }
        }

        private static string SerializeCommand(CommandBase command)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(CommandBase));

                StringBuilder stringBuilder = new StringBuilder();

                using (XmlWriter writer = XmlWriter.Create(stringBuilder))
                    serializer.WriteObject(writer, command);

                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error while serializing command.", ex);

                return null;
            }
        }
    }
}
