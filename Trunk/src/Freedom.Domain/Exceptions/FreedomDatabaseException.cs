using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    public enum FreedomDatabaseErrorCode
    {
        Unknown,

        UnsupportedDatabaseVersion,
        
        DatabaseConnectionFailed,
        
        UnableToCreateDatabase
    }

    [Serializable]
    public class FreedomDatabaseException : Exception
    {
        public FreedomDatabaseException()
        {
        }

        public FreedomDatabaseException(FreedomDatabaseErrorCode errorCode)
            : base(GetErrorMessage(errorCode))
        {
            ErrorCode = errorCode;
        }

        public FreedomDatabaseException(FreedomDatabaseErrorCode errorCode, Exception inner)
            : base(GetErrorMessage(errorCode), inner)
        {
            ErrorCode = errorCode;
        }

        public FreedomDatabaseException(string message) : base(message)
        {
        }

        public FreedomDatabaseException(string message, Exception inner) : base(message, inner)
        {
        }

        public FreedomDatabaseErrorCode ErrorCode
        {
            get; set;
        }

        protected FreedomDatabaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = (FreedomDatabaseErrorCode) info.GetInt32("ErrorCode");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ErrorCode", (int) ErrorCode);
        }

        private static string GetErrorMessage(FreedomDatabaseErrorCode errorCode)
        {
            switch (errorCode)
            {
                case FreedomDatabaseErrorCode.UnsupportedDatabaseVersion:
                    return "This version of the DBMS was is not supported.";

                default:
                    return "An unexpected error occured";
            }
        }
    }
}
