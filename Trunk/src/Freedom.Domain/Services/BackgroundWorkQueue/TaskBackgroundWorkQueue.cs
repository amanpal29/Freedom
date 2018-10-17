using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Freedom.Domain.Services.BackgroundWorkQueue
{
    public class TaskBackgroundWorkQueue : IBackgroundWorkQueue, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CancellationTokenSource _shutdownTokenSource = new CancellationTokenSource();

        private int _lastJobId;
        private int _tasksInQueue;
        private readonly ManualResetEvent _isIdle = new ManualResetEvent(true);

        public void QueueItem(Action<CancellationToken> workItem)
        {
            QueueItem(ct => Task.Run(() => workItem(ct), ct));
        }

        public async void QueueItem(Func<CancellationToken, Task> workItem)
        {
            if (_shutdownTokenSource.IsCancellationRequested)
            {
                Log.Warn("Not starting new background task because the queue is shutting down.");
                return;
            }

            Interlocked.Increment(ref _tasksInQueue);

            _isIdle.Reset();

            int jobId = Interlocked.Increment(ref _lastJobId);

            try
            {
                Log.Info($"Starting background task with jobId {jobId}");

                await workItem(_shutdownTokenSource.Token);

                Log.Info($"Background task with jobId {jobId} completed successfully");
            }
            catch (OperationCanceledException)
            {
                Log.Info($"Background task with jobId {jobId} was cancelled");
            }
            catch (Exception ex)
            {
                Log.Warn($"An unexpected exception was thrown by the background task with with jobId {jobId}", ex);
            }
            finally
            {
                if (Interlocked.Decrement(ref _tasksInQueue) == 0)
                    _isIdle.Set();
            }
        }

        public void Shutdown(TimeSpan timeout)
        {
            _shutdownTokenSource.Cancel();

            if (_tasksInQueue == 0)
                return;

            Log.Info("Signaling background tasks to cancel.");

            if (_isIdle.WaitOne(timeout))
                Log.Info("All background tasks were successfully cancelled.");
            else
                Log.Warn($"{_tasksInQueue:N} background task(s) were still running after waiting {timeout.TotalSeconds:n0} seconds for them to cancel.");
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing) return;

            if (!_shutdownTokenSource.IsCancellationRequested)
                Shutdown(new TimeSpan(0, 30, 0));

            _shutdownTokenSource.Dispose();
            _isIdle.Dispose();
        }
    }
}
