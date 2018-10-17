using System;
using System.ComponentModel;

namespace Freedom.Domain.Infrastructure.Reports
{
    /// <summary>
    /// GroupLevelPropertyDescriptor is purely a place holder.
    /// 
    /// If you use this property in a group header or footer, the property bindings will be
    /// replaced at runtime with the current grouping hierarchy.
    /// </summary>
    public class GroupLevelPropertyDescriptor : PropertyDescriptor
    {
        public GroupLevelPropertyDescriptor()
            : base("Group Level", null)
        {
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            return "Group Level";
        }

        public override void ResetValue(object component)
        {

        }

        public override void SetValue(object component, object value)
        {

        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType => typeof (object);

        public override bool IsReadOnly => false;

        public override Type PropertyType => typeof (string);
    }
}