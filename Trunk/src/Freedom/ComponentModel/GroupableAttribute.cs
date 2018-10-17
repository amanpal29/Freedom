using System;

namespace Freedom.ComponentModel
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class GroupableAttribute : Attribute
    {
        public static readonly GroupableAttribute Yes = new GroupableAttribute(true);
        public static readonly GroupableAttribute No = new GroupableAttribute(false);
        public static readonly GroupableAttribute Default = Yes;

        public GroupableAttribute(bool groupable)
        {
            Groupable = groupable;
        }

        public bool Groupable { get; }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            GroupableAttribute other = obj as GroupableAttribute;

            return (other != null) && other.Groupable == Groupable;
        }

        public override int GetHashCode()
        {
            return Groupable.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return (Equals(Default));
        }
    }
}
