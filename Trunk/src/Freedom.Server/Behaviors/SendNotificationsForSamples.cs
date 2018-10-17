using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Net.Mail;
using System.Reflection;
using Freedom.Domain.Model;
using Freedom.Domain.Model.Behaviors;
using Freedom.Server.Notification;
using Freedom.BackgroundWorker;
using log4net;

namespace Freedom.Server.Behaviors
{
    public class SendNotificationsForSamples : IEntityCommitBehavior
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public void AfterCommit(IList<EntityBase> entities, ObjectContext context)
        {
            using (FreedomLocalContext db = IoC.Get<FreedomLocalContext>())
            {
                List<MailAddress> recipients = new List<MailAddress>();
                EmailNotificationJob job = new EmailNotificationJob(new Guid());

                foreach (MailAddress mailAddress in recipients)
                { 
                   job.To.Add(mailAddress);

                   JobQueue jobQueue = IoC.Get<JobQueue>();

                   jobQueue.Enqueue(job);
                }
            }
        }

        private static void MergeMailingList(ISet<MailAddress> recipients, string addresses)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(addresses)) return;

                MailAddressCollection collection = new MailAddressCollection();

                collection.Add(addresses);

                foreach (MailAddress mailAddress in collection)
                    recipients.Add(mailAddress);
            }
            catch (FormatException exception)
            {
                Log.Warn(string.Format("An error occurred parsing this email address list {0}.", addresses), exception);
            }
        }
             
    }
}
