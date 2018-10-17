using System;

namespace Freedom.ComponentModel
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class ReportableAttribute : Attribute
    {
        public static readonly ReportableAttribute Default = new ReportableAttribute(false);

        public ReportableAttribute()
        {
            Reportable = true;
        }

        public ReportableAttribute(bool reportable)
        {
            Reportable = reportable;
        }

        public bool? Reportable { get; }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            ReportableAttribute other = obj as ReportableAttribute;

            return (other != null) && other.Reportable == Reportable;
        }

        public override int GetHashCode()
        {
            return Reportable.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return Equals(Default);
        }
    }
}
