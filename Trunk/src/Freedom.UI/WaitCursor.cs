using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Freedom.UI
{
    public class WaitCursor : IWaitCursor
    {
        private static int _waitCursorCount;
        private volatile bool _isDisposed;
        private readonly object _padlock = new object();

        public WaitCursor()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                Interlocked.Increment(ref _waitCursorCount);
            }
            catch (InvalidOperationException)
            {
                // If setting OverrideCursor failed (for example if this is constructed from a MTA unit test thread),
                // just mark it as already disposed so we don't mess with the _waitCursorCount.

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _isDisposed) return;

            lock (_padlock)
            {
                if (_isDisposed) return;

                _isDisposed = true;

                if (Interlocked.Decrement(ref _waitCursorCount) == 0)
                    Mouse.OverrideCursor = null;
            }
        }
    }
}
