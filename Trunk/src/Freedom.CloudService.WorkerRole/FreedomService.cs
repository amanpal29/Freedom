using Freedom.WebApi;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Freedom.CloudService.WorkerRole
{
    class FreedomService
    {
        public const string Name = "Freedom";
                
        public static void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration configuration = new HttpConfiguration();

            HttpListener listener = appBuilder.GetProperty<HttpListener>();

            listener.AuthenticationSchemes =
                AuthenticationSchemes.Anonymous |
                AuthenticationSchemes.Basic |
                AuthenticationSchemes.Ntlm |
                AuthenticationSchemes.Negotiate;

            listener.Realm = Name;

            WebApiConfig.Register(configuration);
            appBuilder.UseWebApi(configuration);
        }
    }

    public static class AppBuilderExtensions
    {
        public static T GetProperty<T>(this IAppBuilder appBuilder)
        {
            return (T)appBuilder.Properties[typeof(T).FullName];
        }
    }
}
