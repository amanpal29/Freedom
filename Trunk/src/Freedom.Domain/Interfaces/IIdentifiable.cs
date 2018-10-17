using System;

namespace Freedom.Domain.Interfaces
{
    public interface IIdentifiable
    {
        Guid Id { get; }
    }
}
