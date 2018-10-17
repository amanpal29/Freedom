using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Freedom.WebApi.Infrastructure;
using Freedom.WebApi.Results;
using Freedom.Domain.Services.Security;
using Freedom.WebApi.Models;
using Freedom.Cryptography;

namespace Freedom.WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class FreedomBasicAuthenticationAttribute : FilterAttribute, IAuthenticationFilter
    {
        private const string BasicAuthorizationScheme = "Basic";

        private static readonly ConcurrentDictionary<string, string> PasswordCache =
            new ConcurrentDictionary<string, string>();

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;

            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            if (authorization == null || authorization.Scheme != BasicAuthorizationScheme)
            {
                // No authentication was attempted (for this authentication method).
                // Do not set either Principal (which would indicate success) or ErrorResult (indicating an error).
                return;
            }

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                context.ErrorResult = new AuthenticationFailureResult("Missing basic credentials", request);
                return;
            }

            string userName, password;

            if (!TryExtractUserNameAndPassword(authorization.Parameter, out userName, out password))
            {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                context.ErrorResult = new AuthenticationFailureResult("Invalid basic credentials", request);
                return;
            }

            FreedomUser freedomUser = await GetValidFreedomUserAsync(userName, password);

            if (freedomUser == null)
            {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                context.ErrorResult = new AuthenticationFailureResult("Invalid username or password", request);

                // Add a random delay between 1 and 3 seconds to mitigate brute force attacks.
                Random random = new Random();

                // ReSharper disable once MethodSupportsCancellation
                // (We obviously don't want this delay to be cancellable)
                await Task.Delay(random.Next(1000, 3000));

                return;
            }

            // This user is authenticated. Create a HedgehogIdentity and wrap it in a GenericPrincipal 
            // The HedgehogAuthenticaionFilter will take generate the actual HedgehogPrincipal.
            FreedomIdentity identity = new FreedomIdentity(freedomUser.UserName);

            identity.ForcePasswordChange = freedomUser.ForcePasswordChange;

            context.Principal = new GenericPrincipal(identity, new string[0]);
        }

        private static bool TryExtractUserNameAndPassword(string authorizationParameter, 
            out string userName, out string password)
        {
            userName = password = null;

            // The currently approved HTTP 1.1 specification says characters here are ISO-8859-1.
            // We decode with UTF-8 instead to allow for internationalization of passwords.
            // This works fine with the HedgehogClient because it also uses UTF-8 encoding.
            // If you're ever using something else as a client (e.g. a browser), you either need to
            // either not use passwords with non-ASCII characters, use a browser that also uses UTF-8,
            // or better yet, use a different authentication system (e.g. FormsAuthentication)
            //
            // DGG 2016-02-12

            string decodedCredentials;

            try
            {
                byte[] credentialBytes = Convert.FromBase64String(authorizationParameter);

                decodedCredentials = Encoding.UTF8.GetString(credentialBytes);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }

            if (string.IsNullOrEmpty(decodedCredentials))
                return false;

            int colonIndex = decodedCredentials.IndexOf(':');

            if (colonIndex == -1)
                return false;

            userName = decodedCredentials.Substring(0, colonIndex);
            password = decodedCredentials.Substring(colonIndex + 1);

            return true;
        }

        private static bool IsPasswordValid(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
                return false;

            string validatedPassword;

            if (PasswordCache.TryGetValue(passwordHash, out validatedPassword) && password == validatedPassword)
                return true;

            if (!PasswordUtility.VerifyPasswordHash(password, passwordHash))
                return false;

            PasswordCache[passwordHash] = password;

            return true;
        }

        private static async Task<FreedomUser> GetValidFreedomUserAsync(string userName, string password)
        {
            IFreedomUserCache freedomUserCache = IoC.Get<IFreedomUserCache>();

            FreedomUser freedomUser = freedomUserCache.GetUserDataFromCache(userName);

            if (freedomUser != null && IsPasswordValid(password, freedomUser.PasswordHash))
                return freedomUser;

            freedomUser = await freedomUserCache.GetFreedomUserFromDatabaseAsync(userName);

            if (freedomUser != null && IsPasswordValid(password, freedomUser.PasswordHash))
                return freedomUser;

            return null;
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
