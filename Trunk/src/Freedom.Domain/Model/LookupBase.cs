using System;
using Freedom.Domain.Interfaces;

namespace Freedom.Domain.Model
{
    public partial class LookupBase : IComparable, IComparable<LookupBase>, IOrderable
    {
        public int CompareTo(object obj)
        {
            return obj is LookupBase ? CompareTo((LookupBase) obj) : int.MinValue;
        }

        public int CompareTo(LookupBase that)
        {
            if (SortOrder != that.SortOrder)
                return SortOrder.CompareTo(that.SortOrder);

            string thisValue = Description ?? string.Empty;
            string thatValue = that.Description ?? string.Empty;

            return string.Compare(thisValue, thatValue, StringComparison.InvariantCulture);
        }

        public override string ToString()
        {
            return Description ?? string.Empty;
        }
    }
}

