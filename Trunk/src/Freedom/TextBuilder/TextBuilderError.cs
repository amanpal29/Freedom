using System;
using System.ComponentModel;
using System.Reflection;

namespace Freedom.TextBuilder
{
    public struct TextBuilderError : IEquatable<TextBuilderError>
    {
        public TextBuilderError(int row, int column, int index, TextBuilderErrorCode code)
        {
            Row = row;
            Column = column;
            Index = index;
            Code = code;
        }

        public int Row { get; }

        public int Column { get; }

        public int Index { get; }

        public TextBuilderErrorCode Code { get; }

        #region Equality members

        public static bool operator ==(TextBuilderError left, TextBuilderError right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextBuilderError left, TextBuilderError right)
        {
            return !left.Equals(right);
        }

        #endregion

        public bool Equals(TextBuilderError other)
        {
            return Row == other.Row && Column == other.Column && Index == other.Index && Code == other.Code;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is TextBuilderError && Equals((TextBuilderError) obj);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Row;
                hashCode = (hashCode*397) ^ Column;
                hashCode = (hashCode*397) ^ Index;
                hashCode = (hashCode*397) ^ (int) Code;
                return hashCode;
            }
        }

        public override string ToString()
        {
            FieldInfo field = typeof(TextBuilderErrorCode).GetField(Code.ToString());

            DescriptionAttribute attribute =
                (DescriptionAttribute) Attribute.GetCustomAttribute(field, typeof (DescriptionAttribute));

            return $"({Row},{Column}) {attribute.Description}";
        }
    }
}
