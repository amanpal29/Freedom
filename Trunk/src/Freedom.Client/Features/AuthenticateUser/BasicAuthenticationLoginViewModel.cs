using Freedom.Client.Infrastructure.ExceptionHandling;
using Freedom.Domain.Exceptions;
using Freedom.Domain.Services.Security;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.Client.Features.AuthenticateUser
{
    internal class BasicAuthenticationLoginViewModel : LoginViewModel
    {
        private readonly IHttpClientErrorHandler _errorHandler = IoC.Get<IHttpClientErrorHandler>();

        protected override async Task<bool> TryLoginAsync(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || userName.Contains(":"))
                return false;

            HttpClient httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri(App.BaseAddress, UriKind.Absolute);
            httpClient.Timeout = new TimeSpan(0, 0, 10);

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes($"{userName}:{password}");

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));

                using (HttpResponseMessage response = await httpClient.GetAsync("security/principal"))
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            AuthenticationStatusMessage = response.ReasonPhrase;
                            return false;

                        case (HttpStatusCode)419:
                            PasswordPolicy passwordPolicy = await response.Content.ReadAsAsync<PasswordPolicy>();
                            throw new PasswordExpiredException(response.ReasonPhrase, passwordPolicy);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        AuthenticationStatusMessage = $"Login Error ({response.StatusCode})";
                        return false;
                    }

                    FreedomPrincipal principal = await response.Content.ReadAsAsync<FreedomPrincipal>();

                    if (principal.UserId == Guid.Empty)
                        return false;

                    FreedomCredentials identity = new FreedomCredentials(userName, password);

                    App.User = new FreedomPrincipal(identity, principal);

                    return true;
                }
            }
            catch (TaskCanceledException) // Timeout
            {
                AuthenticationStatusMessage = $"{App.Name} server did not respond.";
            }
            catch (Exception ex)
            {
                if (!_errorHandler.HandleException(httpClient, ex))
                    throw;
            }

            return false;
        }
        
    }
}
