using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    public enum NotificationFailureReason
    {
        None,
        Unknown,
        UnexpectedException,
        InvalidFromAddress,
        MissingSmtpServerAddress,
        InvalidRootEntity,
        MissingToAddress
    }

    [Serializable]
    public class EmailNotificationFailedException : Exception
    {
        public EmailNotificationFailedException()
        {
        }

        public EmailNotificationFailedException(string message)
            : base(message)
        {
        }

        public EmailNotificationFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public EmailNotificationFailedException(NotificationFailureReason reason, string message)
            : base(message)
        {
            Reason = reason;
        }

        public EmailNotificationFailedException(NotificationFailureReason reason, string message, Exception inner)
            : base(message, inner)
        {
            Reason = reason;
        }

        protected EmailNotificationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Reason = (NotificationFailureReason) info.GetInt32(nameof(Reason));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Reason), (int) Reason);
        }

        private NotificationFailureReason Reason { get; }
    }
}
