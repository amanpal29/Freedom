using System;

namespace Freedom.ComponentModel
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SearchWeightAttribute : Attribute
    {
        public static readonly SearchWeightAttribute Default = new SearchWeightAttribute(1.0d);

        public SearchWeightAttribute(double searchWeight)
        {
            SearchWeight = searchWeight;
        }

        public double SearchWeight { get; }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            SearchWeightAttribute other = obj as SearchWeightAttribute;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return other?.SearchWeight == SearchWeight;
        }

        public override int GetHashCode()
        {
            return SearchWeight.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return Equals(Default);
        }
    }
}
