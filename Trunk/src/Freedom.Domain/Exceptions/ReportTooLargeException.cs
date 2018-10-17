using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class ReportTooLargeException : Exception
    {
        public ReportTooLargeException()
        {
        }

        public ReportTooLargeException(string message) : base(message)
        {
        }

        public ReportTooLargeException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ReportTooLargeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
