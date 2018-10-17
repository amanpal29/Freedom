using System;
using System.Linq;

namespace Freedom.Domain.Services.Time
{
    public class CachingTimeServiceDecorator : ITimeService
    {
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksBetweenUpdates = TicksPerMinute * 30;

        private readonly ITimeService _innerTimeService;

        private long _deltaTicks;
        private long _lastUpdateTicks;

        public CachingTimeServiceDecorator(ITimeService innerTimeService)
        {
            _innerTimeService = innerTimeService;
        }

        private void UpdateDeltaTicks()
        {
            try
            {
                // We make four attempts to measure the time difference between local time and the innerTimeService
                // (presumably the server).  We'll discard the first attempt because it might be skewed by DNS
                // lookups, dynamic routing, etc... and use the average of the remaining three.  If the inner TimeService
                // fails with an exception (e.g. the server isn't available because we're offline), we'll just fall back
                // the local clock.

                long[] deltas = new long[4];

                for (int i = 0; i < deltas.Length; i++)
                {
                    long startTicks = DateTime.UtcNow.Ticks;
                    long innerTicks = _innerTimeService.UtcNow.Ticks;
                    long endTicks = DateTime.UtcNow.Ticks;

                    long midTripTicks = (startTicks + endTicks) / 2;

                    deltas[i] = innerTicks - midTripTicks;
                }

                _lastUpdateTicks = DateTime.UtcNow.Ticks;
                _deltaTicks = deltas.Skip(1).Sum() / (deltas.Length - 1);
            }
            catch // If it failed, e.g. innerTimeService wasn't available, fall back on the local clock.
            {
                _lastUpdateTicks = DateTime.UtcNow.Ticks;
                _deltaTicks = 0;
            }
        }

        public DateTime UtcNow
        {
            get
            {
                if (DateTime.UtcNow.Ticks - _lastUpdateTicks > TicksBetweenUpdates)
                    UpdateDeltaTicks();

                return DateTime.UtcNow.AddTicks(_deltaTicks);
            }
        }

        public DateTime Today => DateTime.SpecifyKind(DateTime.Now.AddTicks(_deltaTicks).Date, DateTimeKind.Utc);

        public DateTimeOffset Now
        {
            get
            {
                if (DateTime.UtcNow.Ticks - _lastUpdateTicks > TicksBetweenUpdates)
                    UpdateDeltaTicks();

                return DateTimeOffset.Now.AddTicks(_deltaTicks);
            }
        }
    }
}
