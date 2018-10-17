using System;
using System.Data.Entity.Infrastructure.Interception;
using System.IO;
using System.Reflection;
using Freedom.Domain.Infrastructure;
using Freedom.WebApi.Infrastructure;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Domain.Services.BackgroundWorkQueue;
using Freedom.Domain.Services.Security;
using Freedom.BackgroundWorker;
using Freedom.DependancyInversion;
using Freedom.EntityFramework;
using Freedom.SystemData;
using Freedom.Server.Infrastructure;
using Freedom.WebApi;
using log4net;

namespace Freedom.Server.Config
{
    public static class ServerBootstrapper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string ApplicationServerTierName = "Application Server";

        public static void Bootstrap()
        {
            Log.Info("Bootstrapping FreedomServer...");

            // Reset the container
            Container container = new Container();
            IoC.Container = container;

            // Exception Handler Service
            container.Add<IExceptionHandler>(new LoggingExceptionHandler());
            container.Use<IExceptionHandlerService>(new ExceptionHandlerService());

            // ClientContext
            container.Use<IClientContext>(new OwinClientContext());

            // BackgroundWorkQueue
            container.Use<IBackgroundWorkQueue>(new TaskBackgroundWorkQueue());

            // Client Application Metadata Cache
            string entryLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "C:\\";
            container.Use<IApplicationMetadataCache>(new ApplicationMetadataCache(Path.Combine(entryLocation, "Client")));

            // Register the WebApi services
            container.RegisterAssembly(typeof(WebApiBootstrapper).Assembly);

#if DEBUG
            // Add the logging interceptor to Entity Framework, this logs all SQL server commands that EF6 generates.
            DbInterception.Add(new LoggingInterceptor());
#endif           

            // System Data
            container.Add<ISystemDataProvider>(new WindowsServiceDataProvider(ApplicationServerTierName));
            
            container.Add<PeriodicWorker>(new IndexingEngine());
            
            // Scan assembly for defaults...
            container.ScanWithDefaultConventions(Assembly.GetExecutingAssembly());

            // Scan for plugins
            container.ScanAssembliesInPath(AppDomain.CurrentDomain.BaseDirectory, "freedom.server.*.plugin.dll");
        }
    }
}
