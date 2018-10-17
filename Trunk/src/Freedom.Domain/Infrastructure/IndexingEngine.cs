using System;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Freedom.BackgroundWorker;
using log4net;

namespace Freedom.Domain.Infrastructure
{
    public class IndexingEngine : PeriodicWorker
    {
        private const string IndexTimeOfDayKey = "IndexTimeOfDay";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly TimeSpan DefaultTimeOfDay = new TimeSpan(1, 0, 0); // aka 1 am

        private DateTime _nextRunTime;

        public IndexingEngine()
        {
            Interval = new TimeSpan(0, 5, 0);  // Check every 5 minutes if we need to run...

            TimeSpan indexTimeOfDay;

            string indexTimeOfDayAsString = ConfigurationManager.AppSettings[IndexTimeOfDayKey];

            if (!TimeSpan.TryParse(indexTimeOfDayAsString, out indexTimeOfDay) || 
                indexTimeOfDay < TimeSpan.Zero || indexTimeOfDay.Ticks >= TimeSpan.TicksPerDay)
            {
                indexTimeOfDay = DefaultTimeOfDay;
            }

            _nextRunTime = DateTime.Today + indexTimeOfDay;

            while (_nextRunTime > DateTime.Now)
                _nextRunTime = _nextRunTime.AddDays(-1);
        }

        protected override async Task DoPeriodicWorkAsync(object arg, CancellationToken cancellationToken)
        {
            // If it's not time to run yet
            if (DateTime.Now < _nextRunTime) return;

            Log.Debug("FullTextSearch Indexing Engine thread woke up.");

            await ReindexAllTablesAsync(cancellationToken);

            while (_nextRunTime < DateTime.Now)
                _nextRunTime = _nextRunTime.AddDays(1);

            Log.Debug("FullTextSearch Indexing Engine thread is going back to sleep. "+
                $"The next run time will be shortly after {_nextRunTime:g} server time.");
        }

        private static async Task ReindexAllTablesAsync(CancellationToken cancellationToken)
        {
            
        }
    }
}
