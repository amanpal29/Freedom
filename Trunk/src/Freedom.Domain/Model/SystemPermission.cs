using System.ComponentModel;

namespace Freedom.Domain.Model
{
    public enum SystemPermission
    {
        [Browsable(false)]
        Invalid = 0,       

        #region Users

        [Category("Service Provider")]
        [Description("View Service Providers")]
        ViewUser,

        [Category("Service Provider")]
        [Description("Edit Service Providers")]
        EditUser,

        [Category("Service Provider")]
        [Description("Allow enabling of offline support")]
        EnableOfflineSupport,

        #endregion
    }
}
