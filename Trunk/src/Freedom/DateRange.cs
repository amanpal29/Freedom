using System;

namespace Freedom
{
    public struct DateRange
    {
        public DateRange(DateTime start, DateTime end)
            : this()
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; }

        public DateTime End { get; }

        public TimeSpan Duration => End - Start;

        public bool Contains(DateTime dateTime)
        {
            return Start <= dateTime && dateTime < End;
        }

        public bool Contains(DateRange dateRange)
        {
            return Start <= dateRange.Start && dateRange.Start < End &&
                   Start <= dateRange.End && dateRange.End <= End;
        }

        public bool Intersects(DateRange other)
        {
            // Check for the edge case when one date range is zero length and happens
            // to be on the start date of the other range
            if ((Start == End || other.Start == other.End) && Start == other.Start)
                return true;

                                                                         // Return true if
            return !(Start <= other.Start && End <= other.Start) &&  // This range is not fully before the other range AND
                   !(Start >= other.End && End >= other.End);        // This range is not fully after the other range
        }

        public bool IsAdjacentTo(DateRange other)
        {
            return End == other.Start || Start == other.End;
        }

        public DateRange GetIntersection(DateRange other)
        {
            if (!Intersects(other))
                throw new InvalidOperationException("DateRanges do not intersect");

            return new DateRange(Start >= other.Start ? Start : other.Start,  // The later start date
                                 End < other.End ? End : other.End);          // The eariler end date
        }

        public DateRange GetUnion(DateRange other)
        {
            if (!Intersects(other) && !IsAdjacentTo(other))
                throw new InvalidOperationException("There is a period of time between the DateRanges");

            return new DateRange(Start < other.Start ? Start : other.Start,  // The eariler start date
                                 End >= other.End ? End : other.End);        // the later end date
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            DateRange that = (DateRange) obj;

            return Start == that.Start && End == that.End;
        }

        public static bool operator ==(DateRange left, DateRange right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DateRange left, DateRange right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }

        public override string ToString()
        {
            return $"From {Start} to {End}";
        }
    }
}
