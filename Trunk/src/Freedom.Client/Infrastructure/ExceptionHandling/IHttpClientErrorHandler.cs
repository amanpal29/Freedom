using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;

namespace Freedom.Client.Infrastructure.ExceptionHandling
{
    public interface IHttpClientErrorHandler
    {
        bool HandleException(HttpClient httpClient, Exception exception, CancellationToken token);
        void HandleNonSuccessStatusCode(HttpClient httpClient, HttpStatusCode statusCode, string reasonPhrase, HttpError httpError);
    }

    public static class HttpClientErrorHandlerExtenstions
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool HandleException(this IHttpClientErrorHandler errorHandler, 
            HttpClient httpClient, Exception exception)
        {
            return errorHandler.HandleException(httpClient, exception, CancellationToken.None);
        }

        public static async Task HandleNonSuccessStatusCodeAsync(this IHttpClientErrorHandler errorHandler,
            HttpClient httpClient, HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            HttpError httpError = null;

            try
            {
                httpError = await response.Content.ReadAsAsync<HttpError>();
            }
            catch (UnsupportedMediaTypeException)
            {
                // Content probably isn't an HttpError
            }
            catch (Exception ex)
            {
                Log.Warn(await GetErrorMessageAsync(response.Content), ex);
            }

            errorHandler.HandleNonSuccessStatusCode(httpClient, response.StatusCode, response.ReasonPhrase, httpError);
        }

        private static async Task<string> GetErrorMessageAsync(HttpContent content)
        {
            try
            {
                return "An error occurred trying to deserialise this non-success response content as a HttpError:\n" +
                       await content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return "An error occurred trying to deserialise the non-success response content as a HttpError. " +
                       "The content could also not be logged because another error occurred trying to log it. " +
                       ex.Message;
            }
        }
    }
}