using Freedom.Domain.Exceptions;
using Freedom.Exceptions;
using Freedom.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Freedom.Client.Infrastructure.ExceptionHandling
{
    /// <summary>
    /// The primary function of the HttpClientErrorHandler class is to catch any errors during a
    /// HttpClient request and throw a (hopefully meaningful) Hedgehog.Exceptions.CommunicationException
    /// instead.
    /// 
    /// An HttpClient request can error either a the network level or at the protocol level:
    ///
    ///     Network level errors (Timeouts, DNS lookup failed, SSL/TLS certificate invalid, etc.) 
    ///     occur when a problem happens in OSI layer 1 -> 6. And result in a NetworkCommunicationException
    ///     with a meaningful (to the end user) error message as to what exactly happened.
    /// 
    ///     Protocal level errors (HTTP request returning a 3xx, 4xx or 5xx response)
    ///     occur when the network itself is fine but something happend at layer 7.  They result in
    ///     a HttpStatusCommunicationException.
    /// 
    /// Both types of exceptions are CommunicationException.  So any issue when communicating with the 
    /// server, can be caught and handled by catching this one exception type.
    /// </summary>
    public class HttpClientErrorHandler : IHttpClientErrorHandler
    {
        public bool HandleException(HttpClient httpClient, Exception exception, CancellationToken token)
        {
            if (exception is CommunicationException)
                return false;

            WebException webException = exception.Find<WebException>();

            if (webException != null)
                return HandleWebException(httpClient, webException);

            SocketException socketException = exception.Find<SocketException>();

            if (socketException != null)
                return HandleSocketException(httpClient, exception, socketException);

            TaskCanceledException taskCanceledException = exception as TaskCanceledException;

            if (taskCanceledException != null)
                return HandleTaskCanceledException(httpClient, token);

            return false;
        }

        public void HandleNonSuccessStatusCode(HttpClient httpClient, HttpStatusCode statusCode, string reasonPhrase, HttpError httpError)
        {
            string message = $"The server at {httpClient.BaseAddress} returned an unexpected http status code ({(int) statusCode:D} {reasonPhrase}).";

            switch (statusCode)
            {
                case HttpStatusCode.Forbidden:
                    message = $"The server at {httpClient.BaseAddress} did not allow access to the requested resource. (Error 403)";
                    break;

                case HttpStatusCode.NotFound:
                    message = $"The server at {httpClient.BaseAddress} could not find the requested resource. (Error 404)";
                    break;

                case HttpStatusCode.ProxyAuthenticationRequired:
                    message = "The web proxy requires authentication. (Error 407)";
                    break;

                case HttpStatusCode.RequestTimeout:
                    message = $"The {App.Name} server at {httpClient.BaseAddress} made a request that did not complete within the expected period of time. (Error 408)";
                    break;

                case HttpStatusCode.Conflict:
                    message = $"The {App.Name} server at {httpClient.BaseAddress} couldn't complete the request due to a conflict. (Error 409)";
                    break;

                case HttpStatusCode.Gone:
                    message = $"The {App.Name} server at {httpClient.BaseAddress} couldn't complete the request because the requested resource no longer exists. (Error 410)";
                    break;

                case HttpStatusCode.RequestEntityTooLarge:
                    message = $"The {App.Name} server at {httpClient.BaseAddress} couldn't complete the request because the response was too large to process. (Error 413)";
                    break;

                case HttpStatusCode.InternalServerError:
                    message = $"The {App.Name} server at {httpClient.BaseAddress} encountered an unexpected error while processing the request. (Error 500)";
                    break;

                case HttpStatusCode.NotImplemented:
                    message = $"The {App.Name} server at {httpClient.BaseAddress} couldn't complete the request because the requested api call is not supported on this server. (Error 501)";
                    break;

                case HttpStatusCode.BadGateway:
                    message = $"The proxy server at {httpClient.BaseAddress} couldn't complete the request because it received an invalid response from the upstream server. (Error 502)";
                    break;

                case HttpStatusCode.ServiceUnavailable:
                    message = $"The {App.Name} server at {httpClient.BaseAddress} is temporary unavailable. (Error 503)";
                    break;

                case HttpStatusCode.GatewayTimeout:
                    message = $"The proxy server at {httpClient.BaseAddress} did not receive a response from the upstream server within the expected period of time. (Error 504)";
                    break;

                case HttpStatusCode.HttpVersionNotSupported:
                    message = $"The server at {httpClient.BaseAddress} does not support the version of HTTP used by this client. (Error 505)";
                    break;
            }

            throw new HttpStatusCommunicationException(statusCode, message, GetInnerException(httpError))
            {
                Content = httpError
            };
        }

        private static Exception GetInnerException(HttpError httpError)
        {
            object value;

            Guid uniqueId;

            if (httpError == null ||
                !httpError.TryGetValue("UniqueId", out value) ||
                value == null ||
                !Guid.TryParse(value.ToString(), out uniqueId))
            {
                return null;
            }

            return new ServerException(
                $"This error occurred on the server. Check the server logs for an error with the unique id {uniqueId}",
                uniqueId);
        }

        private static bool HandleWebException(HttpClient httpClient, WebException webException)
        {
            string message = webException.Message;

            switch (webException.Status)
            {
                case WebExceptionStatus.NameResolutionFailure:
                    message = $"The DNS lookup of {httpClient.BaseAddress.Host} failed.";
                    break;

                case WebExceptionStatus.ConnectFailure:
                    message = $"The {App.Name} server at {httpClient.BaseAddress.Host} actively refused a connection on TCP port {httpClient.BaseAddress.Port}.";
                    break;

                case WebExceptionStatus.ReceiveFailure:
                    message = $"The response from the {App.Name} server at {httpClient.BaseAddress} was incomplete.";
                    break;

                case WebExceptionStatus.SendFailure:
                    message = $"A complete request could not be sent to the {App.Name} server at {httpClient.BaseAddress}.";
                    break;

                case WebExceptionStatus.ConnectionClosed:
                    message = $"The connection to the {App.Name} server at {httpClient.BaseAddress} was closed prematurely.";
                    break;

                case WebExceptionStatus.TrustFailure:
                    message = $"The certificate presented by the {App.Name} server at {httpClient.BaseAddress} is not valid.";
                    break;

                case WebExceptionStatus.SecureChannelFailure:
                    message = $"An error occurred while setting up a TLS/SSL connection to the {App.Name} server at {httpClient.BaseAddress.Host}.";
                    break;

                case WebExceptionStatus.Timeout:
                    message = $"The {App.Name} server at {httpClient.BaseAddress.Host} did not respond within the expected period of time.";
                    break;

                case WebExceptionStatus.ProxyNameResolutionFailure:
                    message = "The DNS lookup of the web proxy host name failed.";
                    break;
            }

            throw new NetworkCommunicationException(message, webException);
        }

        private static bool HandleSocketException(HttpClient httpClient, Exception exception, SocketException socketException)
        {
            throw new NetworkCommunicationException(
                $"Unable to communicate with the server at {httpClient.BaseAddress}. {socketException.Message}",
                exception);
        }

        private static bool HandleTaskCanceledException(HttpClient httpClient, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return false;

            string message = $"The {App.Name} server did not respond after waiting {httpClient.Timeout.TotalSeconds:N0} seconds.";

            throw new NetworkCommunicationException(message);
        }
    }
}
