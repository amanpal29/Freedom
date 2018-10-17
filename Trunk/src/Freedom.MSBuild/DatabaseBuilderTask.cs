using System.Data.SqlClient;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Freedom.MSBuild
{
    public abstract class DatabaseBuilderTask : Task
    {
        private string _databaseName;
        private const string DefaultDatabaseName = @"Freedom";

        [Required]
        public string ConnectionString { get; set; }

        public string MasterConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
                builder.InitialCatalog = "master";
                builder.AttachDBFilename = string.Empty;
                return builder.ConnectionString;
            }
        }

        public string DatabaseName
        {
            get
            {
                if (!string.IsNullOrEmpty(_databaseName))
                    return _databaseName;
                
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);

                return string.IsNullOrEmpty(builder.InitialCatalog) ? DefaultDatabaseName : builder.InitialCatalog;
            }
            set
            {
                _databaseName = value;
            }
        }
    }
}