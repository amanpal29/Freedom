using System;
using System.Reflection;
using System.Web.Hosting;
using Freedom.WebApi.Infrastructure;
using Freedom.Domain.Services.BackgroundWorkQueue;
using Freedom.Domain.Services.Security;
using Freedom.WebApp.Infrastructure;
using Freedom.WebApp.Services.BackgroundWorkQueue;
using Freedom.WebApi;
using Freedom.DependancyInversion;
using log4net;

namespace Freedom.WebApp
{
    public static class WebAppBootstrapper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Bootstrap()
        {
            Log.Info("Bootstrapping Freedom.WebApp...");

            // Reset the container
            Container container = new Container();
            IoC.Container = container;

            // ClientContext
            container.Use<IClientContext>(new WebClientContext());

            // Background WorkQueue
            container.Use<IBackgroundWorkQueue>(new AspNetBackgroundWorkQueue());

            // Bootstrapper Metadata
            container.Use<IApplicationMetadataCache>(new ApplicationMetadataCache(HostingEnvironment.MapPath("~/App_Data/Client")));

            // Register the WebApi services
            container.RegisterAssembly(typeof(WebApiBootstrapper).Assembly);

            // Scan assembly for defaults...
            container.ScanWithDefaultConventions(Assembly.GetExecutingAssembly());

            // Scan for plugins
            container.ScanAssembliesInPath(AppDomain.CurrentDomain.BaseDirectory, "freedom.webapp.*.plugin.dll");
        }
    }
}