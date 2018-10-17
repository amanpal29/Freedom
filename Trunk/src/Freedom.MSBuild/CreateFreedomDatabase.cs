using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Threading;
using Freedom.MSBuild.Infrastructure;
using Freedom.Domain.Services.DatabaseBuilder;

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

            if (databaseBuilderService.DatabaseExists)
            {
                Log.LogError($"Unable to create Hedgehog database; database {DatabaseName} already exists.");
                return false;
            }

            databaseBuilderService.CreateDatabaseAsync(CancellationToken.None).Wait();

            return true;
        }
    }
}
