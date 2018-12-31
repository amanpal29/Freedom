using Freedom.Client.Infrastructure.ExceptionHandling;
using Freedom.Domain.Services.Status;
using Freedom.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.Client.Services.Status
{
    public class StatusServiceProxy : IStatusService
    {
        private readonly IHttpClientErrorHandler _httpClientErrorHandler = IoC.Get<IHttpClientErrorHandler>();

        private readonly HttpClient _httpClient;

        public StatusServiceProxy()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(App.BaseAddress, UriKind.Absolute);
            _httpClient.Timeout = new TimeSpan(0, 0, 15);
        }

        public async Task<VersionData> GetVersionDataAsync()
        {
            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync("Status/Version"))
                {
                    await _httpClientErrorHandler.HandleNonSuccessStatusCodeAsync(_httpClient, response);

                    return await response.Content.ReadAsAsync<VersionData>();
                }
            }
            catch (Exception ex)
            {
                _httpClientErrorHandler.HandleException(_httpClient, ex);

                throw;
            }
        }

        public async Task<SystemDataCollection> GetSystemDataAsync()
        {
            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync("Status/SystemData"))
                {
                    await _httpClientErrorHandler.HandleNonSuccessStatusCodeAsync(_httpClient, response);

                    return await response.Content.ReadAsAsync<SystemDataCollection>();
                }
            }
            catch (Exception ex)
            {
                _httpClientErrorHandler.HandleException(_httpClient, ex);

                throw;
            }
        }
    }
}
