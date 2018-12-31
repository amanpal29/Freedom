using Freedom.ViewModels;
using log4net;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Freedom.UI.ViewModels
{
    public class WindowWaitMonitor : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly bool _isBlockingAction;
        private WindowViewModel _windowViewModel;
        private int _isDisposed;

        public WindowWaitMonitor(object owner)
            : this(owner, null)
        {
        }

        public WindowWaitMonitor(object owner, string blockingAction)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            Window window = WindowHelper.FindWindow(owner);

            _isBlockingAction = !string.IsNullOrEmpty(blockingAction);

            if (window == null)
            {
                Log.Debug($"Unable to locate parent window for '{owner}'. WindowWaitMonitor will have no effect.");
                _isDisposed = 1;
                return;
            }

            _windowViewModel = window.DataContext as WindowViewModel;

            if (_windowViewModel == null)
            {
                Log.Debug($"The parent window for '{owner}' isn't using a WindowViewModel. WindowWaitMonitor will have no effect.");
                _isDisposed = 1;
                return;
            }

            _windowViewModel.IncrementAsyncOperationCount();

            if (string.IsNullOrEmpty(blockingAction)) return;

            _isBlockingAction = true;
            _windowViewModel.CurrentBlockingAction = blockingAction;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
                return;

            if (_isBlockingAction)
            {
                _windowViewModel.CurrentBlockingAction = null;
            }

            _windowViewModel.DecrementAsyncOperationCount();
            _windowViewModel = null;
        }
    }
}
