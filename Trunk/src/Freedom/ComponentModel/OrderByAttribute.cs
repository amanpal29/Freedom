using System;

namespace Freedom.ComponentModel
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class OrderByAttribute : Attribute
    {
        public OrderByAttribute(string orderByFormat)
        {
            OrderByFormat = orderByFormat;
        }

        public string OrderByFormat { get; }
    }
}
