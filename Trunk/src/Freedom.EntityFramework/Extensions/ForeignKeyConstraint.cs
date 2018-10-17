using System;

namespace Freedom.Extensions
{
    public struct ForeignKeyConstraint : IEquatable<ForeignKeyConstraint>
    {
        public ForeignKeyConstraint(PropertyReference fromProperty, PropertyReference toProperty)
        {
            From = fromProperty;
            To = toProperty;
        }

        public PropertyReference From { get; }

        public PropertyReference To { get; }

        public string GetFieldForType(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            if (From.Type == entityType.FullName)
                return From.Name;

            if (To.Type == entityType.FullName)
                return To.Name;

            throw new InvalidOperationException(
                $"The type '{entityType.FullName}' is not one of the ends of this ForeignKey.");
        }

        public bool Equals(ForeignKeyConstraint other)
        {
            return From.Equals(other.From) && To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ForeignKeyConstraint && Equals((ForeignKeyConstraint) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (From.GetHashCode()*397) ^ To.GetHashCode();
            }
        }

        public static bool operator ==(ForeignKeyConstraint left, ForeignKeyConstraint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ForeignKeyConstraint left, ForeignKeyConstraint right)
        {
            return !left.Equals(right);
        }
    }
}