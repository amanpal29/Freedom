using System;

namespace Freedom.Extensions
{
    public struct PropertyReference : IEquatable<PropertyReference>
    {
        public PropertyReference(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public string Type { get; }

        public string Name { get; }

        public bool Equals(PropertyReference other)
        {
            return Type == other.Type && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PropertyReference && Equals((PropertyReference) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type.GetHashCode()*397) ^ Name.GetHashCode();
            }
        }

        public static bool operator ==(PropertyReference left, PropertyReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PropertyReference left, PropertyReference right)
        {
            return !left.Equals(right);
        }
    }
}
