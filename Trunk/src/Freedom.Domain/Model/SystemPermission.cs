using System.ComponentModel;

namespace Freedom.Domain.Model
{
    public enum SystemPermission
    {
        [Browsable(false)]
        Invalid = 0,       

        #region Users

        [Category("User")]
        [Description("View User")]
        ViewUser,

        [Category("User")]
        [Description("Edit User")]
        EditUser,

        #endregion
    }
}
