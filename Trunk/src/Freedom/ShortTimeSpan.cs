using System;

namespace Freedom
{
    public struct ShortTimeSpan : IEquatable<ShortTimeSpan>, IComparable, IComparable<ShortTimeSpan>
    {
        public const int MinutesPerHour = 60;
        private const double HoursPerMinute = 1.0d/MinutesPerHour;

        public static readonly ShortTimeSpan Zero = new ShortTimeSpan(0);

        public static readonly ShortTimeSpan MaxValue = new ShortTimeSpan(int.MaxValue);
        public static readonly ShortTimeSpan MinValue = new ShortTimeSpan(int.MaxValue);

        private readonly int _totalMinutes;

        public ShortTimeSpan(int minutes)
        {
            _totalMinutes = minutes;
        }

        public ShortTimeSpan(int hours, int minutes)
        {
            _totalMinutes = hours*MinutesPerHour + minutes;
        }

        public static ShortTimeSpan FromHours(double hours)
        {
            return new ShortTimeSpan((int) Math.Round(hours*MinutesPerHour));
        }

        public static ShortTimeSpan FromTimeSpan(TimeSpan timeSpan)
        {
            return new ShortTimeSpan((int) (timeSpan.Ticks/TimeSpan.TicksPerMinute));
        }

        public static implicit operator ShortTimeSpan(int minutes)
            => new ShortTimeSpan(minutes);

        public static implicit operator int(ShortTimeSpan sts)
            => sts._totalMinutes;

        public long Ticks => _totalMinutes * TimeSpan.TicksPerMinute;

        public int Minutes => _totalMinutes % MinutesPerHour;

        public int Hours => _totalMinutes / MinutesPerHour;

        public int TotalMinutes => _totalMinutes;

        public double TotalHours => _totalMinutes*HoursPerMinute;

        public TimeSpan TimeSpan => new TimeSpan(_totalMinutes * TimeSpan.TicksPerMinute);

        public static explicit operator TimeSpan(ShortTimeSpan ts) => ts.TimeSpan;

        public ShortTimeSpan Add(ShortTimeSpan ts)
        {
            return new ShortTimeSpan(_totalMinutes + ts._totalMinutes);
        }

        public ShortTimeSpan Subtract(ShortTimeSpan ts)
        {
            return new ShortTimeSpan(_totalMinutes - ts._totalMinutes);
        }

        public static int Compare(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            if (t1._totalMinutes > t2._totalMinutes) return 1;
            if (t1._totalMinutes < t2._totalMinutes) return -1;
            return 0;
        }

        public int CompareTo(object value)
        {
            if (value == null) return 1;

            if (!(value is ShortTimeSpan))
                throw new ArgumentException("value must be a ShortTimeSpan", nameof(value));

            return CompareTo((ShortTimeSpan) value);
        }

        public int CompareTo(ShortTimeSpan value)
        {
            long t = value._totalMinutes;
            if (_totalMinutes > t) return 1;
            if (_totalMinutes < t) return -1;
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (!(obj is ShortTimeSpan)) return false;
            return _totalMinutes == ((ShortTimeSpan) obj)._totalMinutes;
        }

        public bool Equals(ShortTimeSpan obj)
        {
            return _totalMinutes == obj._totalMinutes;
        }

        public static bool Equals(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            return t1._totalMinutes == t2._totalMinutes;
        }

        public static ShortTimeSpan operator -(ShortTimeSpan t)
        {
            return new ShortTimeSpan(-t._totalMinutes);
        }

        public static ShortTimeSpan operator -(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            return t1.Subtract(t2);
        }

        public static ShortTimeSpan operator +(ShortTimeSpan t)
        {
            return t;
        }

        public static ShortTimeSpan operator +(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            return t1.Add(t2);
        }

        public static bool operator ==(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            return t1._totalMinutes == t2._totalMinutes;
        }

        public static bool operator !=(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            return t1._totalMinutes != t2._totalMinutes;
        }

        public static bool operator <(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            return t1._totalMinutes < t2._totalMinutes;
        }

        public static bool operator <=(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            return t1._totalMinutes <= t2._totalMinutes;
        }

        public static bool operator >(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            return t1._totalMinutes > t2._totalMinutes;
        }

        public static bool operator >=(ShortTimeSpan t1, ShortTimeSpan t2)
        {
            return t1._totalMinutes >= t2._totalMinutes;
        }

        public override int GetHashCode()
        {
            return _totalMinutes;
        }

        public static ShortTimeSpan Parse(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrWhiteSpace(input))
                throw new FormatException("input can't be empty or whitespace.");

            string[] parts = input.Split(':');

            switch (parts.Length)
            {
                case 1:
                    return new ShortTimeSpan(int.Parse(input));

                case 2:
                    return new ShortTimeSpan(int.Parse(parts[0]), int.Parse(parts[1]));

                default:
                    throw new FormatException("input is not in the correct format.");
            }
        }

        public static bool TryParse(string input, out ShortTimeSpan result)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                string[] parts = input.Split(':');

                switch (parts.Length)
                {
                    case 1:
                        int minutes;
                        if (int.TryParse(input, out minutes))
                        {
                            result = new ShortTimeSpan(minutes);
                            return true;
                        }
                        break;

                    case 2:
                        int hours;
                        if (int.TryParse(parts[0], out hours) && int.TryParse(parts[1], out minutes))
                        {
                            result = new ShortTimeSpan(hours, minutes);
                            return true;
                        }
                        break;
                }
            }

            result = Zero;
            return false;
        }

        public override string ToString()
        {
            return  $"{Hours:D}:{Minutes:D2}";
        }
    }
}
