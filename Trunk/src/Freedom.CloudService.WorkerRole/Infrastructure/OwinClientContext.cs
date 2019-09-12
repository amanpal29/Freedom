using Freedom.Domain.Services.Security;
using Microsoft.Owin;
using System.Net.Http;

namespace Freedom.CloudService.WorkerRole.Infrastructure
{
    public class OwinClientContext : IClientContext
    {
        private const string OwinContext = "MS_OwinContext";

        private static OwinContext GetOwinContext(HttpRequestMessage request)
        {
            if (!request.Properties.ContainsKey(OwinContext))
                return null;

            return (OwinContext)request.Properties[OwinContext];
        }

        public string GetClientAddress(HttpRequestMessage request)
        {
            OwinContext owinContext = GetOwinContext(request);

            return owinContext?.Request?.RemoteIpAddress;
        }
    }
}
