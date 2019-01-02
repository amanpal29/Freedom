using log4net;
using System;
using Freedom.DependancyInversion;
using System.Reflection;
using Freedom.Domain.Exceptions;
using System.Threading.Tasks;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Domain.Services.Time;
using Freedom.Domain.Services.Status;
using Freedom.Domain.Infrastructure;
using Freedom.Domain.Services.BackgroundWorkQueue;
using Freedom.Domain.Services.Query;
using Freedom.Client.Infrastructure.ExceptionHandling;
using Freedom.Client.Infrastructure;
using Freedom.Client.Services.Time;
using Freedom.Client.Services.Status;
using Freedom.Domain.Interfaces;
using Freedom.Domain.Services.Repository;
using Freedom.Domain.Model;
using Freedom.Client.Services.Repository;
using Freedom.Domain.Services.Command;
using Freedom.SystemData;
using Freedom.ViewModels;
using Freedom.Client.Services.Command;
using Freedom.UI;
using Freedom.UI.ViewModels;
using Freedom.Client.Infrastructure.LookupData;
using Freedom.Domain.Model.Definition;

namespace Freedom.Client.Config
{
    public static class ClientBootstrapper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string DefaultProvider = "System.Data.SqlClient";
        private const string ClientTierName = "Client";
        private const string InsuffientPermissionMessage = "You do not have the required permission to perform this operation.  Please contact your Administrator.";

        public static void Bootstrap()
        {
            Log.Info("Bootstrapping FreedomClient...");

            // Reset the container
            Container container = new Container();
            IoC.Container = container;

            // Exception Handling (always register the exception handlers first, so exceptions during bootstrapping are handled)
            container.Add<IExceptionHandler>(new LoggingExceptionHandler());
            container.Add<IExceptionHandler>(new CommunicationExceptionHandler());
            container.Add<IExceptionHandler>(new CustomMessageExceptionHandler(typeof(InsufficientPermissionException), InsuffientPermissionMessage, false));
            container.Add<IExceptionHandler>(new EmptyLookupExceptionHandler());
            container.Add<IExceptionHandler>(new ExceptionSliencer(typeof(CanceledException)));
            container.Add<IExceptionHandler>(new ExceptionSliencer(typeof(TaskCanceledException)));
            container.Add<IExceptionHandler>(new PopUpNullReferenceExceptionSilencer());
            container.Add<IExceptionHandler>(new WindowedExceptionHandler());
            container.Use<IExceptionHandlerService>(new ExceptionHandlerService());
            container.Add<IHttpClientErrorHandler>(new HttpClientErrorHandler());
                        
            // HttpClient
            container.Use<IHttpClientFactory>(new FreedomHttpClientFactory());
            container.Use(() => IoC.Get<IHttpClientFactory>().Create());

            // Time & Status Services
            container.Use<ITimeService>(new CachingNetworkTimeService());
            container.Use<IStatusService>(new StatusServiceProxy());

            // BackgroundWorkQueue
            container.Use<IBackgroundWorkQueue>(new TaskBackgroundWorkQueue());

            // QueryDataProvider
            container.Use(FreedomModelResources.GetMetadataWorkspaceForProvider(DefaultProvider));
            QueryDataProviderCollection dataProviders = new QueryDataProviderCollection();
            dataProviders.Add(new ModelEntityDataProvider());                        
            container.Use<IQueryDataProviderCollection>(dataProviders);

            // Entity Repository
            IEntityRepository entityRepository = new EntityRepositoryAutoRetryDecorator(new EntityRepositoryProxy());
            entityRepository = new EntityRepositoryLoggingDecorator(entityRepository);
            entityRepository = new EntityRepositoryManualRetryDecorator(entityRepository);
            container.Use(entityRepository);
            
            // Command Service
            CommandHandlerCollection commandHandlers = new CommandHandlerCollection();
            commandHandlers.ScanAssemblyForHandlers(typeof(EntityBase).Assembly);
            commandHandlers.ScanAssemblyForHandlers(Assembly.GetExecutingAssembly());
            container.Use<ICommandHandlerCollection>(commandHandlers);
            container.Use<ICommandService>(
                new CommandServiceRefreshDecorator(
                    new CommandServiceManualRetryDecorator(
                        new CommandServiceProxy())));

            // System Data
            container.Add<ISystemDataProvider>(new OperatingSystemDataProvider(ClientTierName));
            container.Add<ISystemDataProvider>(new EnvironmentDataProvider(ClientTierName));
            
            // Awaiting Indicators
            container.Use<IAwaitingIndicatorFactory>(new AwaitingIndicatorFactory());
            container.Use<IWaitCursor>(() => new WaitCursor());

            // Lookup Registration
            container.Use(CreateLookupRegistry());

            // Lookup Table Caches
            container.Use<ILookupRepository>(() => new ApplicationDataLookupRepository());
                                  
            // Scan assembly for defaults...
            container.ScanWithDefaultConventions(Assembly.GetExecutingAssembly());

            // Scan for plugins
            container.ScanAssembliesInPath(AppDomain.CurrentDomain.BaseDirectory, "freedom.client.*.plugin.dll");
        }      

        private static ILookupRegistry CreateLookupRegistry()
        {
            // Lookup Table Resolution Graphs
            
            //LookupTable<WorkArea>.ResolutionGraph = new ResolutionGraph(Paths.WorkArea.ServiceProviders);

            ILookupRegistry lookupRegistry = new LookupRegistry();
                        
            lookupRegistry.Register(new LookupCache<User>(u => u.IsActive && u.Id != User.SuperUserId));            
            //lookupRegistry.Register(new LookupCache<WorkArea>());

            return lookupRegistry;
        }
    }
}
