using System;

namespace Freedom.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime value)
        {
            return value.AddDays(-(int) value.DayOfWeek);
        }

        public static DateTime StartOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1, 0, 0, 0, value.Kind);
        }

        public static DateTime StartOfQuarter(this DateTime value)
        {
            int quarter = (value.Month - 1) / 3;

            return new DateTime(value.Year, quarter * 3 + 1, 1, 0, 0, 0, value.Kind);
        }

        public static DateTime StartOfYear(this DateTime value)
        {
            return new DateTime(value.Year, 1, 1, 0, 0, 0, value.Kind);
        }

        public static DateTime StartOfFiscalYear(this DateTime value, int fiscalYearStartMonth)
        {
            if (fiscalYearStartMonth < 1 || fiscalYearStartMonth > 12)
                throw new ArgumentException("fiscalYearStartMonth must be between 1 and 12.", nameof(fiscalYearStartMonth));

            return value.Month >= fiscalYearStartMonth
                ? new DateTime(value.Year, fiscalYearStartMonth, 1, 0, 0, 0, 0, value.Kind)
                : new DateTime(value.Year - 1, fiscalYearStartMonth, 1, 0,0,0,0, value.Kind);
        }

        public static DateTime EndOfDay(this DateTime value)
        {
            return value.Date.AddTicks(TimeSpan.TicksPerDay - 1);
        }

        public static DateTimeOffset ToLocalTimeWithOffset(this DateTime utcDateTime)
        {
            return new DateTimeOffset(utcDateTime.ToLocalTime());
        }

        public static DateTimeOffset? ToLocalTimeWithOffset(this DateTime? utcDateTime)
        {
            return utcDateTime?.ToLocalTimeWithOffset();
        }

        public static bool EqualsExact(this DateTimeOffset? dateTimeOffset, DateTimeOffset? other)
        {
            // If they're both null, they're equal
            if (dateTimeOffset == null && other == null)
                return true;

            // If either is null (but not both) they're not equal
            if (dateTimeOffset == null || other == null)
                return false;

            // If they're both not null, they're equal if they're exactly equal
            return dateTimeOffset.Value.EqualsExact(other.Value);
        }

        public static bool IsLocalTime(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.EqualsExact(dateTimeOffset.ToLocalTime());
        }

        public static bool IsUtcTime(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.Offset == TimeSpan.Zero;
        }

        public static bool IsInRange(this DateTime dateTime, DateTime start, DateTime end)
        {
            return dateTime >= start && dateTime <= end;
        }
    }
}

