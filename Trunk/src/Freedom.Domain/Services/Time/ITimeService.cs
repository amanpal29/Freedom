using System;

namespace Freedom.Domain.Services.Time
{
    public interface ITimeService
    {
        DateTime UtcNow { get; }
        DateTime Today { get; }
        DateTimeOffset Now { get; }
    }
}
