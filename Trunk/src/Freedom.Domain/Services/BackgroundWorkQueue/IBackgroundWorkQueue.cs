using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Domain.Services.BackgroundWorkQueue
{
    public interface IBackgroundWorkQueue
    {
        void QueueItem(Action<CancellationToken> workItem);

        void QueueItem(Func<CancellationToken, Task> workItem);
    }

    public static class BackgroundWorkQueueExtensions
    {
        public static void QueueItem(this IBackgroundWorkQueue backgroundWorkQueue, Action workItem)
        {
            backgroundWorkQueue.QueueItem(ct => workItem());
        }

        public static void QueueItem(this IBackgroundWorkQueue backgroundWorkQueue, Func<Task> workItem)
        {
            backgroundWorkQueue.QueueItem(ct => workItem());
        }

        public static void QueueItem(this IBackgroundWorkQueue backgroundWorkQueue, IWorkItem workItem)
        {
            backgroundWorkQueue.QueueItem(workItem.ExecuteAsync);
        }

        public static void QueueItems(this IBackgroundWorkQueue backgroundWorkQueue, IEnumerable<IWorkItem> workItems)
        {
            foreach (IWorkItem workItem in workItems)
                backgroundWorkQueue.QueueItem(workItem.ExecuteAsync);
        }
    }
}
