using System.Data.SqlClient;
using Freedom.MSBuild.Infrastructure;
using Freedom.Domain.Services.DatabaseBuilder;
using Microsoft.Build.Framework;

namespace Freedom.MSBuild
{
    public class DeleteDatabase : DatabaseBuilderTask
    {
        public override bool Execute()
        {
            BuildEngineAppender.Register(BuildEngine); // Set Log4net to log to to the current build engine logger.

            DatabaseBuilderService databaseBuilderService = new DatabaseBuilderService();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            builder.InitialCatalog = DatabaseName;
            databaseBuilderService.ProviderConnectionString = builder.ToString();

            if (databaseBuilderService.DatabaseExists)
            {
                databaseBuilderService.DeleteDatabaseAsync().Wait();
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
