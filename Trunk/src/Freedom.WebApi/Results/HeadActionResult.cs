using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Freedom.WebApi.Results
{
    public class HeadActionResult : IHttpActionResult
    {
        public HeadActionResult(IHttpActionResult getActionResult)
        {
            GetActionResult = getActionResult;
        }

        private IHttpActionResult GetActionResult { get; }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            using (HttpResponseMessage originalResponse = await GetActionResult.ExecuteAsync(cancellationToken))
            {
                HttpResponseMessage response = new HttpResponseMessage(originalResponse.StatusCode);

                response.Content = new EmptyContent();

                foreach (KeyValuePair<string, IEnumerable<string>> header in originalResponse.Headers)
                    response.Headers.Add(header.Key, header.Value);

                if (originalResponse.Content == null)
                    return response;

                foreach (KeyValuePair<string, IEnumerable<string>> header in originalResponse.Content.Headers)
                    response.Content.Headers.Add(header.Key, header.Value);

                response.Content.Headers.ContentLength = originalResponse.Content.Headers.ContentLength;

                return response;
            }
        }

        private class EmptyContent : HttpContent
        {
            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return Task.FromResult(0);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0L;

                return true;
            }
        }
    }
}