using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Freedom.Domain.Model.Definition;
using Freedom.Domain.Model;
using Freedom.MSBuild.Infrastructure;
using Freedom.Domain.Services.Command;
using Freedom.Domain.Services.DataLoader;
using Microsoft.Build.Framework;

namespace Freedom.MSBuild
{
    public class ImportFreedomData : DatabaseBuilderTask
    {
        public ImportFreedomData()
        {
            Provider = "System.Data.SqlClient";
        }

        [Required]
        public ITaskItem[] Files { get; set; }

        public string Provider { get; set; }

        private FreedomLocalContext CreateContext()
        {
            EntityConnectionStringBuilder connectionStringBuilder = new EntityConnectionStringBuilder();

            connectionStringBuilder.Provider = Provider;
            connectionStringBuilder.Metadata = FreedomModelResources.GetMetadataForProvider(Provider);
            connectionStringBuilder.ProviderConnectionString = ConnectionString;

            return new FreedomLocalContext(connectionStringBuilder.ToString());
        }

        public override bool Execute()
        {
            BuildEngineAppender.Register(BuildEngine); // Set Log4net to log to to the current build engine logger.

            if (Files == null || Files.Length == 0)
            {
                Log.LogError("Unable to import data, no files were specified.");
                return false;
            }

            foreach (ITaskItem taskItem in Files)
            {
                if (File.Exists(taskItem.ItemSpec)) continue;
                Log.LogError($"Unable to import data. The file '{taskItem.ItemSpec}' was not found.");
                return false;
            }

            string[] fileNames = Files.Select(item => item.ItemSpec).ToArray();            

            if (Provider == "System.Data.SqlClient")
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
                builder.InitialCatalog = DatabaseName;
                ConnectionString = builder.ToString();
            }

            IEntityDataLoader entityDataLoader = IoC.Get<IEntityDataLoader>();

            entityDataLoader.LoadFromFiles(fileNames);

            Log.LogMessage(MessageImportance.Normal, "Committing data to database...");

            using (FreedomLocalContext context = CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                Guid currentUserId = User.SuperUserId;
                DateTime modifyTimeStamp = DateTime.UtcNow;

                foreach (Entity entity in entityDataLoader.Results)
                {
                    AggregateRoot aggregateRoot = entity as AggregateRoot;

                    if (aggregateRoot != null)
                    {
                        aggregateRoot.CreatedBy = null;
                        aggregateRoot.CreatedById = currentUserId;
                        aggregateRoot.CreatedDateTime = modifyTimeStamp;
                        aggregateRoot.ModifiedBy = null;
                        aggregateRoot.ModifiedById = currentUserId;
                        aggregateRoot.ModifiedDateTime = modifyTimeStamp;
                    }

                    context.Add(entity);
                }

                context.OnCommittingAsync(null, new CommandExecutionContext(currentUserId, modifyTimeStamp)).Wait();

                context.SaveChanges();
            }

            return true;
        }
    }
}
