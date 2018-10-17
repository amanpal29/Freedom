using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Freedom.WebApi.Results
{
    public class LocalFileResult : IHttpActionResult
    {
        private const int BufferSize = 1024 * 1024;

        private const string AttachmentDispositionType = "attachment";

        public LocalFileResult(string filePath, string originalFileName = null, string mediaType = null)
        {
            FilePath = filePath;
            OriginalFileName = originalFileName;
            MediaType = mediaType ?? FileUtility.GetMediaType(filePath);
        }

        public string FilePath { get; }

        public string OriginalFileName { get; set; }

        public string MediaType { get; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            Stream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize);

            StreamContent content = new StreamContent(stream, BufferSize);

            ContentDispositionHeaderValue contentDisposition =
                new ContentDispositionHeaderValue(AttachmentDispositionType);

            contentDisposition.FileName = OriginalFileName ?? Path.GetFileName(FilePath);

            content.Headers.ContentDisposition = contentDisposition;

            if (!string.IsNullOrEmpty(MediaType))
                content.Headers.ContentType = new MediaTypeHeaderValue(MediaType);

            response.Content = content;

            return Task.FromResult(response);
        }
    }
}