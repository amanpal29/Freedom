using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Time;
using Freedom.BackgroundWorker;
using Freedom.Domain.Exceptions;
using Freedom.TextBuilder;
using log4net;

namespace Freedom.Server.Notification
{
    public class EmailNotificationJob : Job
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FreedomLocalContext _db = IoC.Get<FreedomLocalContext>();
        private readonly MailMessage _message = new MailMessage();
                
        private readonly Guid _rootEntityId;        

        public EmailNotificationJob(Guid rootEntityId)
        {
            _rootEntityId = rootEntityId;

            _message = new MailMessage();
        }

        public MailAddressCollection To
        {
            get { return _message.To; }
        }

        public MailAddressCollection CC
        {
            get { return _message.CC; }
        }

        public MailAddressCollection Bcc
        {
            get { return _message.Bcc; }
        }

        private static string Format(string formatString, params object[] args)
        {
            TextBuilder.TextBuilder textBuilder = new TextBuilder.TextBuilder(formatString);

            if (textBuilder.Build(args) > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("The following errors occured while generating text for the email:");

                foreach (TextBuilderError error in textBuilder.Errors)
                    stringBuilder.AppendFormat("\t{0}\n", error);

                if (textBuilder.Errors.All(e => e.Code > TextBuilderErrorCode.RunTimeErrors))
                    Log.Info(stringBuilder);
                else
                    Log.Warn(stringBuilder);
            }

            return textBuilder.ToString();
        }

        protected override bool Execute()
        {
            if (_message.To.Count == 0 && _message.CC.Count == 0 && _message.Bcc.Count == 0)
            {
                Log.Info("Not sending email notification because there are no email addresses to send to.");

                CommitEmailNotification(NotificationFailureReason.MissingToAddress);

                return true;
            }

            SmtpClient client = new SmtpClient();

            if (client.DeliveryMethod == SmtpDeliveryMethod.Network && string.IsNullOrWhiteSpace(client.Host))
            {
                CommitEmailNotification(NotificationFailureReason.MissingSmtpServerAddress);

                throw new InvalidOperationException(
                    "The SMTP Host has not been specified in the server's configuration.");
            }            

            try
            {
                string fromAddress = ConfigurationManager.AppSettings.Get("NotificationEmailFrom");

                if (string.IsNullOrWhiteSpace(fromAddress))
                {
                    Log.ErrorFormat("Unable to send email. There is no From email address specified in the config file.");

                    CommitEmailNotification(NotificationFailureReason.InvalidFromAddress);

                    return false;
                }

                _message.From = new MailAddress(fromAddress);
            }
            catch (FormatException exception)
            {
                Log.Error("Unable to send email. The From email address specified in the config file is invalid.", exception);

                CommitEmailNotification(NotificationFailureReason.InvalidFromAddress);

                return false;
            }

            _message.Subject = "Freedom Notification Email";
            
            Log.Debug("Sending email notification");

            client.Send(_message);

            Log.Info("Email notification was successfully send to outgoing mail server.");
            
            CommitEmailNotification(NotificationFailureReason.None);

            return true;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool dispose)
        {
            if (dispose)
            {
                 _db.Dispose();
            }
        }

        private void CommitEmailNotification(NotificationFailureReason notificationFailureReason)
        {
            ITimeService timeService = IoC.Get<ITimeService>() ?? new LocalTimeService();

            //EmailNotification emailNotification = new EmailNotification
            //{
            //    // TODO: Legolas
            //    //CreatedDateTime = timeService.UtcNow,
            //    //CreatedById = User.SuperUserId,
            //    //ModifiedDateTime = timeService.UtcNow,
            //    //ModifiedById = User.SuperUserId,
            //    AggregateRootId = _rootEntityId,
            //    EmailTemplateId = _emailTemplateId,
            //    FailureReason = notificationFailureReason,
            //    State = notificationFailureReason == NotificationFailureReason.None ? NotificationState.Successful : NotificationState.Failed,
            //    SentDateTime = timeService.UtcNow,
            //    SentFrom = ConfigurationManager.AppSettings.Get("NotificationEmailFrom"),
            //    SentTo = string.Join("; ", To.Select(to => to.Address)),
            //    CC = string.Join("; ", CC.Select(cc => cc.Address)),
            //    Bcc = string.Join("; ", Bcc.Select(bcc => bcc.Address)),
            //    Subject = _emailTemplate != null && _rootEntity != null ? Format(_emailTemplate.Subject, _rootEntity) : string.Empty
            //};

            //_db.EmailNotification.Add(emailNotification);
            //_db.SaveChanges();
        }
    }
}
