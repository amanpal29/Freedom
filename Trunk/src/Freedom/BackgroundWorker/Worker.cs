using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.BackgroundWorker
{
    public abstract class Worker
    {
        #region Fields

        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly object _workerPadlock = new object();

        #endregion

        #region Events

        public event ProgressChangedEventHandler ProgressChanged;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        protected virtual void OnProgressChanged(ProgressChangedEventArgs args)
        {
            ProgressChanged?.Invoke(this, args);
        }

        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs args)
        {
            RunWorkerCompleted?.Invoke(this, args);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this thread is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this thread is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning => _workerThread?.IsAlive ?? false;

        /// <summary>
        /// Gets a value indicating whether this thread has been requested to stop.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is requested to stop; otherwise, <c>false</c>.
        /// </value>
        public bool IsStopRequested => _cancellationTokenSource?.IsCancellationRequested ?? false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the worker thread with the supplied argument
        /// </summary>
        public virtual void Start(object argument = null)
        {
            lock (_workerPadlock)
            {
                if (IsRunning)
                    throw new InvalidOperationException("Worker thread is already running.");

                _workerThread = new Thread(WorkerThreadProc);
                _workerThread.IsBackground = true;
                _workerThread.Start(argument);
            }
        }

        /// <summary>
        /// Requests the worker thread to stop.
        /// This won't actually stop the worker thread, it only sets a flag requesting that the worker thread stop.
        /// </summary>
        public virtual void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Requests the worker thread to stop, and waits the specified timeout for it to stop.
        /// </summary>
        public void Stop(TimeSpan timeout)
        {
            if (!IsRunning) return;

            Stop();

            _workerThread?.Join(timeout);
        }

        /// <summary>
        /// Forceably aborts this thread, (use with caution)
        /// </summary>
        public virtual void Abort()
        {
            if (_workerThread?.IsAlive == true)
                _workerThread?.Abort();
        }

        #endregion

        #region Protected Methods

        protected void ReportProgress(int percentProgress)
        {
            OnProgressChanged(new ProgressChangedEventArgs(percentProgress, null));
        }

        protected void ReportProgress(int percentProgress, object userState)
        {
            OnProgressChanged(new ProgressChangedEventArgs(percentProgress, userState));
        }

        protected virtual object DoWork(object arg)
        {
            throw new NotImplementedException("The synchronous method DoWork() has not been implemented.");
        }

        protected virtual Task<object> DoWorkAsync(object arg, CancellationToken cancellationToken)
        {
            return Task.FromResult(DoWork(arg));
        }

        #endregion

        #region Private Methods

        private void WorkerThreadProc(object argument)
        {
            try
            {
                object result = null;
                Exception error = null;
                bool cancelled = false;

                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    try
                    {
                        result = DoWorkAsync(argument, _cancellationTokenSource.Token).Result;

                        if (_cancellationTokenSource.IsCancellationRequested)
                            cancelled = true;
                    }
                    catch (Exception ex)
                    {
                        if (ex is ThreadAbortException || ex is OperationCanceledException)
                            cancelled = true;

                        error = ex;
                    }

                    OnRunWorkerCompleted(new RunWorkerCompletedEventArgs(result, error, cancelled));
                }
            }
            finally
            {
                _workerThread = null;
                _cancellationTokenSource = null;
            }
        }

        #endregion
    }
}
