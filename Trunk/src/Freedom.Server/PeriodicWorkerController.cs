using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Freedom.BackgroundWorker;
using log4net;

namespace Freedom.Server
{
    public class PeriodicWorkerController : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<PeriodicWorker> _periodicWorkers;
        private bool _isDisposed;

        public PeriodicWorkerController()
        {
            _periodicWorkers = IoC.GetAll<PeriodicWorker>().ToList();

            StartAllWorkers();
        }

        private void StartAllWorkers()
        {
            foreach (PeriodicWorker periodicWorker in _periodicWorkers)
            {
                try
                {
                    Log.Info($"Starting worker {periodicWorker.GetType().Name}");

                    periodicWorker.Start();
                }
                catch (Exception ex)
                {
                    Log.Error($"An exception occurred while trying to start worker {periodicWorker.GetType().Name}.", ex);
                }
            }
        }

        private void StopAllWorkers()
        {
            Log.Info("Shutting down all worker threads.");

            foreach (PeriodicWorker periodicWorker in _periodicWorkers)
            {
                try
                {
                    periodicWorker.Stop(new TimeSpan(0, 0, 10));
                }
                catch (Exception exception)
                {
                    Log.Error($"An exception occurred while trying to stop worker {periodicWorker.GetType().Name}.", exception);
                }
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _isDisposed) return;
            _isDisposed = true;
            StopAllWorkers();
        }

        #endregion
    }
}
