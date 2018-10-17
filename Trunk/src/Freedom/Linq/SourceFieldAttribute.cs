using System;

namespace Freedom.Linq
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SourceFieldAttribute : Attribute
    {
        public static SourceFieldAttribute Default = new SourceFieldAttribute();

        private SourceFieldAttribute()
        {
            FieldType = typeof(object);
        }

        public SourceFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
            FieldType = typeof(object);
        }

        public SourceFieldAttribute(string fieldName, Type fieldType)
        {
            FieldName = fieldName;
            FieldType = fieldType;
        }

        public string FieldName { get; }

        public Type FieldType { get; }

        public override bool IsDefaultAttribute()
        {
            return FieldName == default(string) && FieldType == typeof(object);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            SourceFieldAttribute other = obj as SourceFieldAttribute;

            return other != null && other.FieldName == FieldName;
        }

        public bool Equals(SourceFieldAttribute other)
        {
            return string.Equals(FieldName, other.FieldName);
        }

        public override int GetHashCode()
        {
            return (FieldName ?? string.Empty).GetHashCode();
        }
    }
}
