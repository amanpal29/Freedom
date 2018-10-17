using System;

namespace Freedom.FullTextSearch
{
    internal struct TextSubstitution : IEquatable<TextSubstitution>
    {
        private readonly string _find;
        private readonly string _replaceWith;

        public TextSubstitution(string find, string replaceWith)
        {
            if (string.IsNullOrEmpty(find))
                throw new ArgumentNullException(nameof(find));

            if (string.IsNullOrEmpty(replaceWith))
                throw new ArgumentNullException(nameof(replaceWith));

            _find = find;
            _replaceWith = replaceWith;
        }

        public string Find => _find;

        public string ReplaceWith => _replaceWith;

        public bool Equals(TextSubstitution other)
        {
            return string.Equals(_find, other._find) && string.Equals(_replaceWith, other._replaceWith);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TextSubstitution && Equals((TextSubstitution) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_find.GetHashCode()*397) ^ _replaceWith.GetHashCode();
            }
        }

        public static bool operator ==(TextSubstitution left, TextSubstitution right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextSubstitution left, TextSubstitution right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{_find} => {_replaceWith}";
        }
    }
}