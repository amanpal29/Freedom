using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Freedom.Domain.Services.Security;

namespace Freedom.WebApi.Results
{
    public class PasswordMustBeChangedResult : NegotiatedContentResult<PasswordPolicy>
    {
        public PasswordMustBeChangedResult(PasswordPolicy passwordPolicy, ApiController apiController)
            : base((HttpStatusCode) 419, passwordPolicy, apiController)
        {
        }

        public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.ExecuteAsync(cancellationToken);

            response.ReasonPhrase = "Password Expired";

            return response;
        }
    }
}