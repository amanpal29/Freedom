using System;

namespace Freedom.ComponentModel
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DefaultColumnIndexAttribute : Attribute
    {
        public static readonly DefaultColumnIndexAttribute Default = new DefaultColumnIndexAttribute();

        private readonly int _index;

        private DefaultColumnIndexAttribute()
        {
            _index = -1;
        }

        public DefaultColumnIndexAttribute(int index)
        {
            _index = index;
        }

        public int DefaultColumnIndex => _index;

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            DefaultColumnIndexAttribute other = obj as DefaultColumnIndexAttribute;

            return (other != null) && other.DefaultColumnIndex == _index;
        }

        public override int GetHashCode()
        {
            return _index.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return (Equals(Default));
        }
    }
}
