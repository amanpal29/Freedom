using System;

namespace Freedom.IO
{
    public struct ProgressReport : IEquatable<ProgressReport>
    {
        private const double OnePerDay = 1d/86400d;

        public ProgressReport(long position, long length)
            : this(position, length, double.NaN)
        {
        }

        public ProgressReport(long position, long length, double currentRate)
        {
            Position = position;
            Length = length;
            CurrentRate = currentRate;
        }

        public long Position { get; }

        public long Length { get; }

        public double CurrentRate { get; }

        public long Remaining => Position < Length ? Length - Position : 0L;

        public double? PercentComplete => Length > 0 ? Math.Min(Position*100d/Length, 100d) : (double?) null;

        public TimeSpan? EstimatedTimeRemaining
        {
            get
            {
                if (Length == 0)
                    return null;

                if (Remaining == 0L)
                    return TimeSpan.Zero;

                if (double.IsNaN(CurrentRate) || Math.Abs(CurrentRate) < OnePerDay)
                    return null;

                return TimeSpan.FromSeconds(Remaining/CurrentRate);
            }
        }

        public bool Equals(ProgressReport other)
        {
            return Position == other.Position && Length == other.Length && CurrentRate.Equals(other.CurrentRate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ProgressReport && Equals((ProgressReport) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Position.GetHashCode();
                hashCode = (hashCode*397) ^ Length.GetHashCode();
                hashCode = (hashCode*397) ^ CurrentRate.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ProgressReport left, ProgressReport right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProgressReport left, ProgressReport right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return double.IsNaN(CurrentRate) ? $"{Position}/{Length}" : $"{Position}/{Length} ({CurrentRate:f0}/s)";
        }
    }
}
