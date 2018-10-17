using System;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Flags]
    public enum PropertyFlags
    {
        Normal = 0x0,

        StorageOnly = 0x1,

        Independant = 0x2
    }
}
