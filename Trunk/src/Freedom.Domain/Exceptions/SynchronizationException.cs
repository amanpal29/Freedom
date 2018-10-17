using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    public enum SynchronizationExceptionCode
    {
        Unknown = 0,
        NoInitialConfiguration = 1,
        GlobalIdentifierMismatch = 2,
        CouldNotOpenOfflineDatabase = 3,
        CriticalErrorDuringLastChanceUpdate = 4,
        CouldNotUpgradeDatabase = 5
    }

    [Serializable]
    public class SynchronizationException : Exception
    {
        public SynchronizationException()
        {
        }

        public SynchronizationException(string message)
            : base(message)
        {
        }

        public SynchronizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SynchronizationException(SynchronizationExceptionCode code, string message)
            : base(message)
        {
            Code = code;
        }

        public SynchronizationException(SynchronizationExceptionCode code, string message, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
        }

        public SynchronizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Code = (SynchronizationExceptionCode) info.GetInt32(nameof(Code));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Code), (int) Code);
        }

        public SynchronizationExceptionCode Code { get; } 
    }
}
