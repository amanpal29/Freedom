using System;
using System.Runtime.Serialization;
using Freedom.Domain.Services.Security;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class PasswordExpiredException : Exception
    {
        public PasswordExpiredException()
        {
            
        }

        public PasswordExpiredException(string message)
            : base(message)
        {
        }

        public PasswordExpiredException(string message, PasswordPolicy passwordPolicy)
            : base(message)
        {
            MinimumPasswordLength = passwordPolicy.MinimumPasswordLength;
            MinimumPasswordComplexity = passwordPolicy.MinimumPasswordComplexity;
        }

        public PasswordExpiredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PasswordExpiredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MinimumPasswordLength = info.GetInt32("MinimumPasswordLength");
            MinimumPasswordComplexity = info.GetInt32("MinimumPasswordComplexity");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MinimumPasswordLength", MinimumPasswordLength);
            info.AddValue("MinimumPasswordComplexity", MinimumPasswordComplexity);
            base.GetObjectData(info, context);
        }

        public int MinimumPasswordLength { get; set; }

        public int MinimumPasswordComplexity { get; set; }
    }
}