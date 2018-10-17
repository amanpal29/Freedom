using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Reflection;
using Freedom.Domain.Infrastructure;
using Freedom.Domain.Model;
using Freedom.Domain.Model.Definition;
using Freedom.Domain.Services.Command;
using Freedom.Domain.Services.Query;
using Freedom.Domain.Services.Sequence;
using Freedom.Domain.Services.Time;
using Freedom.DependancyInversion;
using Freedom.SystemData;
using Freedom.WebApi.Infrastructure;
using log4net;

namespace Freedom.WebApi
{
    public class WebApiBootstrapper : IRegister
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string ConnectionStringName = "default";
        private const string DefaultProvider = "System.Data.SqlClient";

        private const string ApplicationServerTierName = "Application Server";
        private const string DatabaseServerTierName = "Database Server";

        public void Register(Container container)
        {
            Log.Info("Registering WebApi Configuration...");

            // Time Services
            container.Use<ITimeService>(new LocalTimeService());

            // Database Connection
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            container.Use<IDbConnection>(() => new SqlConnection(connectionStringSettings.ConnectionString));
            container.Use<DbConnection>(() => new SqlConnection(connectionStringSettings.ConnectionString));

            // MetadataWorkspace
            container.Use(
                FreedomModelResources.GetMetadataWorkspaceForProvider(GetProviderName(connectionStringSettings)));

            // HedgehogLocalContext
            container.Use(() => new FreedomLocalContext(GetEntityConnectionString()));

            // Security Services
            container.Use<IFreedomUserCache>(new FreedomUserCache());

            // Notification Factory
            container.Use<INotificationFactory>(new NotificationFactory());

            // Command Service
            CommandHandlerCollection commandHandlers = new CommandHandlerCollection();
            commandHandlers.ScanAssemblyForHandlers(typeof(EntityBase).Assembly);
            commandHandlers.ScanAssemblyForHandlers(Assembly.GetExecutingAssembly());
            container.Use<ICommandHandlerCollection>(commandHandlers);

            // Query Service
            QueryDataProviderCollection dataProviders = new QueryDataProviderCollection();
            dataProviders.Add(new ModelEntityDataProvider());
            //dataProviders.Add(new DigestEntityDataProvider());           
            
            container.Use<IQueryDataProviderCollection>(dataProviders);
                        
            // AutoNumberGenerator
            container.Use<ISequenceGenerator>(new SqlServerSequenceGenerator());
            container.Use<IAutoNumberGenerator, AutoNumberGenerator>();

            // System Data
            container.Add<ISystemDataProvider>(new OperatingSystemDataProvider(ApplicationServerTierName));
            container.Add<ISystemDataProvider>(new EnvironmentDataProvider(ApplicationServerTierName));
            container.Add<ISystemDataProvider>(new SqlServerSystemDataProvider(DatabaseServerTierName,
                () => new SqlConnection(connectionStringSettings.ConnectionString)));
            container.Add<ISystemDataProvider>(new FreedomDatabaseDataProvider(DatabaseServerTierName,
                () => new SqlConnection(connectionStringSettings.ConnectionString)));
                        
            // Scan assembly for defaults...
            container.ScanWithDefaultConventions(Assembly.GetExecutingAssembly());
            
            // Scan for plugins
            container.ScanAssembliesInPath(AppDomain.CurrentDomain.BaseDirectory, "freedom.webapi.*.plugin.dll");
        }

        private static string GetProviderName(ConnectionStringSettings connectionString)
        {
            string provider = connectionString?.ProviderName;

            if (string.IsNullOrWhiteSpace(provider))
                provider = DefaultProvider;

            return provider;
        }

        private static string GetEntityConnectionString()
        {
            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName];

            if (connectionString == null)
                throw new InvalidOperationException("Default connection string was not found in the config file.");

            EntityConnectionStringBuilder connectionStringBuilder = new EntityConnectionStringBuilder();

            connectionStringBuilder.Provider = GetProviderName(connectionString);
            connectionStringBuilder.Metadata =
                FreedomModelResources.GetMetadataForProvider(GetProviderName(connectionString));
            connectionStringBuilder.ProviderConnectionString = connectionString.ConnectionString;

            return connectionStringBuilder.ToString();
        }
    }
}