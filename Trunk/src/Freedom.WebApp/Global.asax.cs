using Freedom.WebApi;
using System.Web.Http;

namespace Freedom.WebApp
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WebAppBootstrapper.Bootstrap();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
