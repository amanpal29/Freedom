using Freedom.Domain.Infrastructure;
using Freedom.Domain.Services.Security;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;

namespace Freedom.Client.Infrastructure
{
    public class FreedomHttpClientFactory : IHttpClientFactory
    {
        private IIdentity _identity;
        private HttpClient _httpClient;

        public HttpClient Create()
        {
            if (_httpClient != null && Equals(_identity, App.User?.Identity))
                return _httpClient;

            _identity = App.User?.Identity;

            FreedomCredentials freedomCredentials = _identity as FreedomCredentials;

            HttpClientHandler handler = new HttpClientHandler();

            if (_identity is WindowsIdentity)
            {
                handler.UseDefaultCredentials = true;
            }
            else if (freedomCredentials != null)
            {
                handler.Credentials = new NetworkCredential(freedomCredentials.Name, freedomCredentials.Password);
            }

            _httpClient = new HttpClient(handler);

            _httpClient.BaseAddress = new Uri(App.BaseAddress, UriKind.Absolute);

            if (freedomCredentials != null)
            {
                string parameter = $"{freedomCredentials.Name}:{freedomCredentials.Password}";

                parameter = Convert.ToBase64String(Encoding.UTF8.GetBytes(parameter));

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", parameter);
            }

            return _httpClient;
        }
    }
}
