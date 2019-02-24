using System.Data.SqlClient;
using Freedom.MSBuild.Infrastructure;
using Freedom.Domain.Services.DatabaseBuilder;
using Microsoft.Build.Framework;
using System;
using System.ComponentModel;
using Microsoft.Identity.Client;

namespace Freedom.MSBuild
{
    public class DeleteDatabase : DatabaseBuilderTask
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

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            builder.InitialCatalog = DatabaseName;
            databaseBuilderService.ProviderConnectionString = builder.ToString();
            databaseBuilderService.FreedomDatabaseType = _freedomDatabaseType;
            databaseBuilderService.ServerName = ServerName;
            databaseBuilderService.SubscriptionId = AzureSubscriptionId;
            databaseBuilderService.ResourceGroupName = ResourceGroupName;

            if (databaseBuilderService.DatabaseExists)
            {
                AuthenticationResult authenticationResult = null;
                if (_freedomDatabaseType == FreedomDatabaseType.Cloud)
                {
                    AzureAuthenticator authenticator = new AzureAuthenticator(AzureClientId, ClientSecret, AzureAuthority, AzureRedirectUri);
                    authenticationResult = authenticator.GetToken(null);
                }
                databaseBuilderService.DeleteDatabaseAsync(authenticationResult != null ? authenticationResult.AccessToken : string.Empty).Wait();
            }
            else
            {
                Log.LogMessage(MessageImportance.Normal,
                    $"Skipping DeleteDatabase task; database '{DatabaseName}' has already been deleted.");
            }

            return !databaseBuilderService.DatabaseExists;
        }
    }
}
