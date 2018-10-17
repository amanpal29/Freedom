using System;

namespace Freedom.Domain.Services.Time
{
    public class LocalTimeService : ITimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime Today => DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
