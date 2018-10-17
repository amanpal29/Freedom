using System;

namespace Freedom.ComponentModel
{
    [Flags]
    public enum FilterStyle
    {
        None = 0,
        DistinctValues = 1
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class FilterStyleAttribute : Attribute
    {        
        public static readonly FilterStyleAttribute Default = new FilterStyleAttribute(FilterStyle.None);

        public FilterStyleAttribute(FilterStyle filterStyle)
        {
            FilterStyle = filterStyle;
        }

        public FilterStyle FilterStyle { get; }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            FilterStyleAttribute other = obj as FilterStyleAttribute;

            return (other != null) && other.FilterStyle == FilterStyle;
        }

        public override int GetHashCode()
        {
            return FilterStyle.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return (Equals(Default));
        }
    }
}
