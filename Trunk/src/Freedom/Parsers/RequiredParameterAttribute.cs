using System;

namespace Freedom.Parsers
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredParameterAttribute : Attribute
    {
    }
}