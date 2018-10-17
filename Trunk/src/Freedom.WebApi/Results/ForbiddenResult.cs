using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Freedom.WebApi.Results
{
    public class ForbiddenResult : IHttpActionResult
    {
        public ForbiddenResult(HttpRequestMessage request)
        {
            Request = request;
        }

        public HttpRequestMessage Request { get; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.Forbidden;
            response.RequestMessage = Request;
            return response;
        }
    }
}