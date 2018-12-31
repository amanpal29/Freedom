using Freedom.ViewModels;
using System;

namespace Freedom.UI.ViewModels
{
    public class AwaitingIndicatorFactory : IAwaitingIndicatorFactory
    {
        public IDisposable Create(object owner)
        {
            return owner != null ? new WindowWaitMonitor(owner) : null;
        }
    }
}
