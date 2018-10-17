using System;

namespace Freedom.Domain.Infrastructure.Reports
{
    [Flags]
    public enum ModelDomains
    {
        Entity = 0x1,
        Digest = 0x2,
        CompletionRate = 0x4,
        WaterDistribution = 0x8
    }
}
