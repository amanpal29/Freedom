using System.Data.SqlClient;
using Freedom.MSBuild.Infrastructure;
using Freedom.Domain.Services.DatabaseBuilder;
using Microsoft.Build.Framework;
using System;
using System.ComponentModel;
using System.Threading;

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

            if (databaseBuilderService.DatabaseExists)
            {                
                databaseBuilderService.DeleteDatabaseAsync(CancellationToken.None, string.Empty).Wait();
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
