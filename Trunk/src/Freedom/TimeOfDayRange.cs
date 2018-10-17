using System;

namespace Freedom
{
    public struct TimeOfDayRange : IEquatable<TimeOfDayRange>
    {
        public static readonly TimeOfDayRange Closed = new TimeOfDayRange();
        public static readonly TimeOfDayRange Open24Hours = new TimeOfDayRange(TimeSpan.Zero, new TimeSpan(TimeSpan.TicksPerDay - 1));

        public TimeOfDayRange(TimeSpan openTime, TimeSpan closeTime)
        {
            if (openTime.Ticks < 0L || openTime.Ticks >= TimeSpan.TicksPerDay)
                throw new ArgumentException("openTime must be between 00:00:00 and 23:59:59.9999999", nameof(openTime));

            if (closeTime.Ticks < 0L || closeTime.Ticks > TimeSpan.TicksPerDay)
                throw new ArgumentException("closeTime must be between 00:00:00 and 24:00:00", nameof(closeTime));

            if (closeTime.Ticks == 0L && openTime.Ticks > 0L)
                closeTime = new TimeSpan(TimeSpan.TicksPerDay);

            if (closeTime.Ticks == TimeSpan.TicksPerDay && openTime.Ticks == 0L)
                closeTime = new TimeSpan(TimeSpan.TicksPerDay - 1);

            if (closeTime < openTime)
                throw new ArgumentException("openTime must be before closeTime");

            OpenTime = openTime;
            CloseTime = closeTime;
        }

        public static TimeOfDayRange FromTimes(TimeSpan openTime, TimeSpan closeTime)
        {
            return new TimeOfDayRange(openTime, closeTime);
        }

        public static TimeOfDayRange? FromTimes(TimeSpan? openTime, TimeSpan? closeTime)
        {
            if (openTime == null || closeTime == null)
                return null;

            return new TimeOfDayRange(openTime.Value, closeTime.Value);
        }

        public bool IsClosed => OpenTime == CloseTime;

        public bool IsOpen24Hours => OpenTime.Ticks == 0L && CloseTime.Ticks == TimeSpan.TicksPerDay -1;

        public TimeSpan OpenTime { get; }

        public TimeSpan CloseTime { get; }

        public TimeSpan Duration => CloseTime - OpenTime;

        public bool IsOpenAt(TimeSpan timeOfDay)
            => OpenTime <= timeOfDay && timeOfDay < CloseTime;

        public bool Equals(TimeOfDayRange other)
        {
            return OpenTime.Equals(other.OpenTime) && CloseTime.Equals(other.CloseTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TimeOfDayRange && Equals((TimeOfDayRange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (OpenTime.GetHashCode()*397) ^ CloseTime.GetHashCode();
            }
        }

        public static bool operator ==(TimeOfDayRange left, TimeOfDayRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimeOfDayRange left, TimeOfDayRange right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            if (IsClosed)
                return "- Closed -";

            if (IsOpen24Hours)
                return "Open 24 Hours";

            return $"{OpenTime:hh:mm:ss} to {CloseTime:hh:mm:ss}";
        }
    }
}
