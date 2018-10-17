using System;

namespace DemoDataBuilder.ComponentModel
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class AlternateNameAttribute : Attribute
    {
        public static readonly AlternateNameAttribute Default = new AlternateNameAttribute();

        private readonly string _alternateName;

        public AlternateNameAttribute()
            : this(string.Empty)
        {
        }

        public AlternateNameAttribute(string alternateName)
        {
            _alternateName = alternateName;
        }

        public string AlternateName
        {
            get { return _alternateName; }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            AlternateNameAttribute other = obj as AlternateNameAttribute;

            return (other != null) && other.AlternateName == AlternateName;
        }

        public override int GetHashCode()
        {
            return AlternateName.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return (this.Equals(Default));
        }
    }
}
