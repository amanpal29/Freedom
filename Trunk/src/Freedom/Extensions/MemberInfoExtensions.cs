using System;
using System.ComponentModel;
using System.Reflection;
using Freedom.ComponentModel;

namespace Freedom.Extensions
{
    public static class MemberInfoExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this MemberInfo member)
            where TAttribute : Attribute
        {
            return (TAttribute) Attribute.GetCustomAttribute(member, typeof (TAttribute));
        }

        public static bool IsBrowsable(this MemberInfo member)
        {
            BrowsableAttribute attribute =
                (BrowsableAttribute) Attribute.GetCustomAttribute(member, typeof (BrowsableAttribute));

            return attribute == null || attribute.Browsable;
        }

        public static bool IsGroupable(this MemberInfo member)
        {
            GroupableAttribute attribute =
                (GroupableAttribute) Attribute.GetCustomAttribute(member, typeof (GroupableAttribute));

            return attribute == null || attribute.Groupable;
        }

        public static bool IsReportable(this MemberInfo member)
        {
            ReportableAttribute attribute =
                (ReportableAttribute) Attribute.GetCustomAttribute(member, typeof (ReportableAttribute));

            return attribute != null && attribute.Reportable.GetValueOrDefault();
        }

        public static string GetDisplayName(this MemberInfo member)
        {
            DisplayNameAttribute attribute =
                (DisplayNameAttribute) Attribute.GetCustomAttribute(member, typeof (DisplayNameAttribute));

            return attribute != null ? attribute.DisplayName : member.Name.ToDisplayName();
        }
    }
}
