using System;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Freedom.Extensions;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class NetworkCommunicationException : CommunicationException
    {
        public NetworkCommunicationException()
        {
        }

        public NetworkCommunicationException(string message)
            : base(message)
        {
        }

        public NetworkCommunicationException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }

        protected NetworkCommunicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public bool IsTimeout => InnerException is TaskCanceledException;

        public WebExceptionStatus? WebExceptionStatus => InnerException?.Find<WebException>()?.Status;
    }
}
