using System;

namespace Freedom.ViewModels
{
    public interface IAwaitingIndicatorFactory
    {
        IDisposable Create(object owner);
    }
}
