using System;

namespace Freedom.ComponentModel
{
    /// <summary>
    /// Identifies an entity type as "Confidential" and identifies its related public data
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConfidentialAttribute : Attribute
    {
        public static bool IsDefined(Type type) => type.IsDefined(typeof (ConfidentialAttribute), true);

        public static ConfidentialAttribute GetAttribute(Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof (ConfidentialAttribute), true);

            return (ConfidentialAttribute) (attributes.Length > 0 ? attributes[0] : null);
        }

        public ConfidentialAttribute(Type publicType)
        {
            PublicType = publicType;
        }

        public Type PublicType { get; set; }
    }
}
