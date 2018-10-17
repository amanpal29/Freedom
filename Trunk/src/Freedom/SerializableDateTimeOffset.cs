using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Freedom
{
    enum SerializableDateTimeOffsetStatus
    {
        Valid = 1,
        ParseError
    };

    public struct SerializableDateTimeOffset :
        IComparable, IComparable<DateTimeOffset>, IComparable<SerializableDateTimeOffset>,
        IEquatable<DateTimeOffset>, IEquatable<SerializableDateTimeOffset>,
        IFormattable, IXmlSerializable
    {
        #region Field

        private DateTimeOffset _value;

        private SerializableDateTimeOffsetStatus _status;

        #endregion

        #region Constructors
        public SerializableDateTimeOffset(DateTimeOffset value)
        {
            _value = value;
            _status = SerializableDateTimeOffsetStatus.Valid;
        }

        public SerializableDateTimeOffset(long ticks, TimeSpan offset)
        {
            _value = new DateTimeOffset(ticks, offset);
            _status = SerializableDateTimeOffsetStatus.Valid;
        }

        public SerializableDateTimeOffset(DateTime dateTime)
        {
            _value = new DateTimeOffset(dateTime);
            _status = SerializableDateTimeOffsetStatus.Valid;
        }

        public SerializableDateTimeOffset(DateTime dateTime, TimeSpan offset)
        {
            _value = new DateTimeOffset(dateTime, offset);
            _status = SerializableDateTimeOffsetStatus.Valid;
        }

        public SerializableDateTimeOffset(int year, int month, int day, int hour, int minute, int second,
            TimeSpan offset)
        {
            _value = new DateTimeOffset(year, month, day, hour, minute, second, offset);
            _status = SerializableDateTimeOffsetStatus.Valid;
        }

        public SerializableDateTimeOffset(int year, int month, int day, int hour, int minute, int second,
            int millisecond, TimeSpan offset)
        {
            _value = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, offset);
            _status = SerializableDateTimeOffsetStatus.Valid;
        }

        #endregion

        #region Properties

        public DateTime Date => _value.Date;

        public DateTime DateTime => _value.DateTime;

        public DateTimeOffset DateTimeOffset => _value;

        public DateTime UtcDateTime => _value.UtcDateTime;

        public DateTime LocalDateTime => _value.LocalDateTime;

        public int Day => _value.Day;

        public DayOfWeek DayOfWeek => _value.DayOfWeek;

        public int DayOfYear => _value.DayOfYear;

        public int Hour => _value.Hour;

        public int Millisecond => _value.Millisecond;

        public int Minute => _value.Minute;

        public int Month => _value.Month;

        public TimeSpan Offset => _value.Offset;

        public int Second => _value.Second;

        public long Ticks => _value.Ticks;

        public long UtcTicks => _value.UtcTicks;

        public TimeSpan TimeOfDay => _value.TimeOfDay;

        public int Year => _value.Year;

        public bool HasParseError => _status == SerializableDateTimeOffsetStatus.ParseError;

        public bool IsMissing => _status != SerializableDateTimeOffsetStatus.ParseError && _value == new DateTimeOffset();

        #endregion

        #region Equality and Comparison Methods

        public static int Compare(SerializableDateTimeOffset first, SerializableDateTimeOffset second)
        {
            return DateTimeOffset.Compare(first._value, second._value);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (obj is DateTimeOffset)
                return _value.CompareTo((DateTimeOffset) obj);

            if (obj is SerializableDateTimeOffset)
                return _value.CompareTo(((SerializableDateTimeOffset) obj)._value);

            throw new ArgumentException("obj must be a DateTimeOffset or a SerializableDateTimeOffset", nameof(obj));
        }

        public int CompareTo(DateTimeOffset other)
        {
            return _value.CompareTo(other);
        }

        public int CompareTo(SerializableDateTimeOffset other)
        {
            return _value.CompareTo(other._value);
        }

        public override bool Equals(object obj)
        {
            if (obj is DateTimeOffset)
                return Equals((DateTimeOffset) obj);

            if (obj is SerializableDateTimeOffset)
                return Equals((SerializableDateTimeOffset) obj);

            return false;
        }

        public bool Equals(DateTimeOffset other)
        {
            return _value.Equals(other);
        }

        public bool Equals(SerializableDateTimeOffset other)
        {
            return _value.Equals(other._value);
        }

        public bool EqualsExact(DateTimeOffset other)
        {
            return _value.EqualsExact(other);
        }

        public bool EqualsExact(SerializableDateTimeOffset other)
        {
            return _value.EqualsExact(other._value);
        }

        public static bool Equals(SerializableDateTimeOffset first, SerializableDateTimeOffset second)
        {
            return DateTimeOffset.Equals(first._value, second._value);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            // _value can only be set when deserializing, so it's effectivly readonly.
            return _value.GetHashCode();
        }

        public static bool operator ==(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
        {
            return left._value == right._value;
        }

        public static bool operator !=(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
        {
            return left._value != right._value;
        }

        public static bool operator <(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
        {
            return left._value < right._value;
        }

        public static bool operator <=(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
        {
            return left._value <= right._value;
        }

        public static bool operator >(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
        {
            return left._value > right._value;
        }

        public static bool operator >=(SerializableDateTimeOffset left, SerializableDateTimeOffset right)
        {
            return left._value >= right._value;
        }

        #endregion

        #region Implicit Type Conversion

        public static implicit operator SerializableDateTimeOffset(DateTime dateTime)
        {
            return new SerializableDateTimeOffset(dateTime);
        }

        public static implicit operator SerializableDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            return new SerializableDateTimeOffset(dateTimeOffset);
        }

        public static implicit operator DateTimeOffset(SerializableDateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset._value;
        }

        #endregion

        #region Implementation of IFormattable

        public override string ToString()
        {
            return _value.ToString();
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return _value.ToString(formatProvider);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return _value.ToString(format, formatProvider);
        }

        #endregion

        #region Implementation of IXmlSerializable

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            string temporaryValue = reader.ReadElementString();
            try
            {
                _value = DateTimeOffset.Parse(temporaryValue);
            }
            catch (FormatException)
            {
                _value = new DateTimeOffset();
                _status = SerializableDateTimeOffsetStatus.ParseError;
                // do not rethrow exception, because that would prevent parsing of the rest of the XML file
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if (_value.Offset == TimeSpan.Zero)
                writer.WriteString(_value.UtcDateTime.ToString("o"));

            writer.WriteString(_value.ToString("o"));
        }

        #endregion
    }
}
