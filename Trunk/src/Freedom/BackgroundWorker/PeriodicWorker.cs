using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Annotations;
using log4net;

namespace Freedom.BackgroundWorker
{
    /// <summary>
    /// A background worker that wakes up periodically to perform some task.
    /// </summary>
    public abstract class PeriodicWorker : Worker, INotifyPropertyChanged, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isDisposed;
        private DateTime _startTime = DateTime.MinValue;
        private TimeSpan _interval = new TimeSpan(0, 10, 0);
        private readonly AutoResetEvent _waitHandler = new AutoResetEvent(false);
        private bool _isWorking;

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// Gets or sets the amount of time from the start of one periodic work event to the start of the next.
        /// </summary>
        /// <value>
        /// The work interval.
        /// </value>
        public TimeSpan Interval
        {
            get { return _interval; }
            set
            {
                if (_interval == value) return;
                _interval = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is currently working.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is working; otherwise, <c>false</c>.
        /// </value>
        public bool IsWorking
        {
            get { return _isWorking; }
            private set
            {
                if (value == _isWorking) return;
                _isWorking = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSleeping));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is currently sleeping.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is sleeping; otherwise, <c>false</c>.
        /// </value>
        public bool IsSleeping => !IsWorking;

        /// <summary>
        /// Requests the worker thread to stop.
        /// This won't actually stop the worker thread, it only sets a flag requesting that the worker thread stop.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            WakeUpNow();
        }

        /// <summary>
        /// Causes this worker to wake up and start working immediatly.
        /// If the worker is already working, this does nothing.
        /// </summary>
        public void WakeUpNow()
        {
            if (IsSleeping)
                _waitHandler.Set();
        }

        protected virtual void DoPeriodicWork(object arg)
        {
            throw new NotImplementedException("The synchronous method DoPeriodicWork() has not been implemented.");
        }

        protected virtual Task DoPeriodicWorkAsync(object arg, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                DoPeriodicWork(arg);

            return Task.FromResult(0);
        }

        #region Overrides of Worker

        protected override async Task<object> DoWorkAsync(object arg, CancellationToken cancellationToken)
        {
            while (!IsStopRequested)
            {
                try
                {
                    try
                    {
                        _startTime = DateTime.UtcNow;

                        IsWorking = true;

                        await DoPeriodicWorkAsync(arg, cancellationToken);
                    }
                    finally
                    {
                        IsWorking = false;
                    }
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    Log.Warn("An unhandled exception occurred in a periodic worker.", exception);

                    if (IsStopRequested)
                        throw;
                }

                if (IsStopRequested) break;

                TimeSpan sleepTime = _startTime + _interval - DateTime.UtcNow;

                if (sleepTime > TimeSpan.Zero)
                    _waitHandler.WaitOne(sleepTime);
            }

            return null;
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (!disposing) return;

            _waitHandler.Dispose();

            _isDisposed = true;
        }

        #endregion
    }
}
