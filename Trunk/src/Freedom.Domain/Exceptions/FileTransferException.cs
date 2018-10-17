using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class FileTransferException : Exception
    {
        public FileTransferException()
        {
        }

        public FileTransferException(string message)
            : base(message)
        {
        }

        public FileTransferException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public FileTransferException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}