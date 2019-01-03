using System;

namespace Freedom.Domain.Model
{
    public partial class User : IComparable, IComparable<User>
    {
        public static Guid SuperUserId = new Guid("{DA378215-F0A8-4F47-AD19-48002D0B8E67}");

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
