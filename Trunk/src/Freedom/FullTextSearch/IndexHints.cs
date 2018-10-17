using System;

namespace Freedom.FullTextSearch
{
    [Flags]
    public enum IndexHints
    {
        None = 0x0,

        Ignore = 0x1,

        Force = 0x2,

        Identifier = 0x10,

        Address = 0x100,

        PhoneNumber = 0x200,

        PostalCode = 0x400,

        Email = 0x800
    }
}
