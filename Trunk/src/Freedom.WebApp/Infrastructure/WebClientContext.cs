using System.Net.Http;
using System.Web;
using Freedom.Domain.Services.Security;

namespace Freedom.WebApp.Infrastructure
{
    public class WebClientContext : IClientContext
    {
        private const string HttpContext = "MS_HttpContext";

        private static HttpContextWrapper GetHttpContextWrapper(HttpRequestMessage request)
        {
            if (!request.Properties.ContainsKey(HttpContext))
                return null;

            return request.Properties[HttpContext] as HttpContextWrapper;
        }

        public string GetClientAddress(HttpRequestMessage request)
        {
            return GetHttpContextWrapper(request)?.Request.UserHostAddress;
        }
    }
}