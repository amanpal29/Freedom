using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Threading;
using Freedom.MSBuild.Infrastructure;
using Freedom.Domain.Services.DatabaseBuilder;
using Microsoft.Identity.Client;

namespace Freedom.MSBuild
{
    public class CreateFreedomDatabase : DatabaseBuilderTask
    {
        private FreedomDatabaseType _freedomDatabaseType = FreedomDatabaseType.Server;

        public string DatabaseType
        {
            get { return _freedomDatabaseType.ToString(); }
            set
            {
                if (!Enum.TryParse(value, true, out _freedomDatabaseType))
                    throw new InvalidEnumArgumentException(
                        $"The value {value} is not valid for the DatabaseType property.");
            }
        }

        public override bool Execute()
        {
            BuildEngineAppender.Register(BuildEngine); // Set Log4net to log to to the current build engine logger.
                        
            DatabaseBuilderService databaseBuilderService = new DatabaseBuilderService();
            databaseBuilderService.FreedomDatabaseType = _freedomDatabaseType;

            Log.LogMessage($"Building {_freedomDatabaseType} database...");

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            builder.InitialCatalog = DatabaseName;
            databaseBuilderService.ProviderConnectionString = builder.ToString();
            databaseBuilderService.ServerName = ServerName;
            databaseBuilderService.SubscriptionId = AzureSubscriptionId;
            databaseBuilderService.ResourceGroupName = ResourceGroupName;

            if (databaseBuilderService.DatabaseExists)
            {
                Log.LogError($"Unable to create Hedgehog database; database {DatabaseName} already exists.");
                return false;
            }

            AuthenticationResult authenticationResult = null;
            if (_freedomDatabaseType == FreedomDatabaseType.Cloud)
            {                
                string[] scopes = new string[] { "https://management.azure.com/.default" };
                AzureAuthenticator authenticator = new AzureAuthenticator(AzureClientId, ClientSecret, AzureAuthority, AzureRedirectUri);
                authenticationResult = authenticator.GetToken(scopes);
            }
            
            databaseBuilderService.CreateDatabaseAsync(CancellationToken.None, authenticationResult != null ? authenticationResult.AccessToken : string.Empty).Wait();

            return true;
        }
    }
}
