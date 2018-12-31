using Freedom.Client.Infrastructure.ExceptionHandling;
using Freedom.Domain.Services.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.Client.Services.Time
{
    class CachingNetworkTimeService : ITimeService, IAsyncInitializable
    {
        private readonly IHttpClientErrorHandler _errorHandler = IoC.Get<IHttpClientErrorHandler>();

        private long _deltaTicks;

        public async Task InitializeCoreAsync()
        {
            _deltaTicks = 0L;
            IsSynchronized = false;

            // We make four attempts to measure the time difference between local time and the remote server's time.

            // We'll discard the first attempt because it might be skewed by DNS lookups, dynamic routing,
            // Authentication, SSH tunneling, whatever... and use the average of the remaining three. 

            // If this fails with an exception (e.g. the server isn't available because we're offline),
            // we'll just log it and fall back on the local clock.

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(App.BaseAddress, UriKind.Absolute);
            httpClient.Timeout = new TimeSpan(0, 0, 15);

            long[] deltas = new long[4];

            for (int i = 0; i < deltas.Length; i++)
            {
                DateTime remoteDateTime;

                long startTicks = DateTime.UtcNow.Ticks;

                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync("Time"))
                    {
                        await _errorHandler.HandleNonSuccessStatusCodeAsync(httpClient, response);

                        remoteDateTime = await response.Content.ReadAsAsync<DateTime>();
                    }
                }
                catch (Exception ex)
                {
                    _errorHandler.HandleException(httpClient, ex);

                    throw;
                }

                long endTicks = DateTime.UtcNow.Ticks;

                long midTripTicks = (startTicks + endTicks) / 2;

                deltas[i] = remoteDateTime.Ticks - midTripTicks;
            }

            _deltaTicks = deltas.Skip(1).Sum() / (deltas.Length - 1);

            IsSynchronized = true;

        }

        public bool IsInitialized { get; set; }

        public bool IsSynchronized { get; private set; }

        public DateTime UtcNow => DateTime.UtcNow.AddTicks(_deltaTicks);

        public DateTime Today => DateTime.SpecifyKind(DateTime.Now.AddTicks(_deltaTicks).Date, DateTimeKind.Utc);

        public DateTimeOffset Now => DateTimeOffset.Now.AddTicks(_deltaTicks);
    }
}
