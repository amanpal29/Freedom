using System.IO;
using System.Net;
using System.Web.Http;
using Freedom.WebApi.Infrastructure;
using Freedom.WebApi.Models;
using Freedom.WebApi.Results;

namespace Freedom.WebApi.Controllers
{
    [AllowAnonymous]
    public class BootstrapController : ApiController
    {
        private readonly IApplicationMetadataCache _applicationMetadataCache = IoC.TryGet<IApplicationMetadataCache>();

        [Route("bootstrap")]
        public IHttpActionResult Get()
        {
            ApplicationMetadata metadata = _applicationMetadataCache?.GetApplicationMetadata();

            if (metadata == null)
                return StatusCode(HttpStatusCode.ServiceUnavailable);

            return Ok(metadata);
        }

        [Route("bootstrap/{id}")]
        public IHttpActionResult Get(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Contains("..") || id.StartsWith("/"))
                return BadRequest();

            string filePath = _applicationMetadataCache?.GetLocalFilePath(id);

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return NotFound();

            return new LocalFileResult(filePath);
        }
    }
}
