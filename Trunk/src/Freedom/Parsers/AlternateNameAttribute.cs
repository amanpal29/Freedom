using System;

namespace Freedom.Parsers
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AlternateNameAttribute : Attribute, IEquatable<AlternateNameAttribute>
    {
        public AlternateNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public bool Equals(AlternateNameAttribute other)
        {
            return other != null && other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            return obj == this || Equals(obj as AlternateNameAttribute);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
