using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Freedom.WebApi.Results
{
    public class OkNegotiatedContentResult : IHttpActionResult
    {
        public OkNegotiatedContentResult(Type contentType, object content, ApiController controller)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            if (contentType == null)
                throw new ArgumentNullException(nameof(content));

            if (!contentType.IsInstanceOfType(content))
                throw new ArgumentException("contant must be an instance of contentType", nameof(content));

            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            ContentType = contentType;

            Content = content;

            Request = controller.Request;

            if (Request == null)
                throw new ArgumentException("controller.Request must not be null", nameof(controller));

            HttpConfiguration configuration = controller.Configuration;

            if (configuration == null)
                throw new ArgumentException("controller.Configuration must not be null", nameof(controller));

            ContentNegotiator = configuration.Services.GetContentNegotiator();

            if (ContentNegotiator == null)
                throw new ArgumentException("Could not find IContentNegotiator in the controller.Services", nameof(controller));

            Formatters = configuration.Formatters;

            if (Formatters == null)
                throw new ArgumentException("controller.Formatters must not be null", nameof(controller));
        }

        public Type ContentType { get; }

        public object Content { get; }

        public HttpRequestMessage Request { get; }

        public IContentNegotiator ContentNegotiator { get; }

        public IEnumerable<MediaTypeFormatter> Formatters { get; }

        public HttpResponseMessage Execute()
        {
            ContentNegotiationResult negotiationResult = ContentNegotiator.Negotiate(ContentType, Request, Formatters);

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

            try
            {
                if (negotiationResult == null)
                {
                    httpResponseMessage.StatusCode = HttpStatusCode.NotAcceptable;
                }
                else
                {
                    httpResponseMessage.StatusCode = HttpStatusCode.OK;

                    httpResponseMessage.Content = new ObjectContent(ContentType, Content, negotiationResult.Formatter, negotiationResult.MediaType);
                }

                httpResponseMessage.RequestMessage = Request;
            }
            catch
            {
                httpResponseMessage.Dispose();

                throw;
            }

            return httpResponseMessage;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }
    }
}