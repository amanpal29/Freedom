using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Freedom.Domain.Services.BackgroundWorkQueue;

namespace Freedom.WebApp.Services.BackgroundWorkQueue
{
    public class AspNetBackgroundWorkQueue : IBackgroundWorkQueue
    {
        public void QueueItem(Action<CancellationToken> workItem)
        {
            HostingEnvironment.QueueBackgroundWorkItem(workItem);
        }

        public void QueueItem(Func<CancellationToken, Task> workItem)
        {
            HostingEnvironment.QueueBackgroundWorkItem(workItem);
        }

        public void Shutdown(TimeSpan timeSpan)
        {
            // Intentional no-op
            // The HostingEnvironment will shutdown the BackgroundWorkQueue
        }
    }
}
