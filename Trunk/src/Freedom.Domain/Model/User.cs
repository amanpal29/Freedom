using System;

namespace Freedom.Domain.Model
{
    public partial class User : IComparable, IComparable<User>
    {
        public static Guid SuperUserId = new Guid("{3B526C4E-50F3-425D-9787-6DB0696290FF}");

        public static string DisplayNameHandler(User u)
        {
            return string.IsNullOrEmpty(u.Name) ? u.Username : u.Name;
        }

        public User()
        {
            _forcePasswordChange = true;
        }

        public string DisplayName => DisplayNameHandler(this);

        public string FullName
            => DisplayName; 
       

        #region Implementation of IComparable

        public int CompareTo(object obj)
        {
            return obj is User ? CompareTo((User)obj) : int.MinValue;
        }

        public int CompareTo(User other)
        {
            return string.Compare(Name, other.Name, StringComparison.CurrentCulture);
        }

        #endregion
    }
}
