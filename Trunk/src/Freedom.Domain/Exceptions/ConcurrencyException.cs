using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Freedom.Extensions;

namespace Freedom.Domain.Exceptions
{
    public enum ConcurrencyExceptionCode
    {
        None,

        [Description("The entity was not found in the database; it may have been deleted.")]
        ItemNotFound
    }

    [Serializable]
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException()
        {
        }

        public ConcurrencyException(string message)
            : base(message)
        {
        }

        public ConcurrencyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ConcurrencyException(ConcurrencyExceptionCode code)
            : base(EnumEx.GetDisplayName(code))
        {
            Code = code;
        }

        public ConcurrencyException(ConcurrencyExceptionCode code, Exception inner)
            : base(EnumEx.GetDisplayName(code), inner)
        {
            Code = code;
        }

        protected ConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Code = (ConcurrencyExceptionCode) info.GetInt32(nameof(Code));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Code), (int) Code);
        }

        public ConcurrencyExceptionCode Code { get; }
    }
}
