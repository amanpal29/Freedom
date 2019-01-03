using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Exceptions;
using Freedom.Domain.Model;
using Freedom.Domain.Model.Definition;
using Freedom.Annotations;
using Freedom.Cryptography;
using Freedom.Extensions;
using Freedom.FullTextSearch;
using log4net;

namespace Freedom.Domain.Services.DatabaseBuilder
{
    public class DatabaseBuilderService : IDatabaseBuilderService, INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string DefaultDatabaseName = @"Freedom";

        private const int SqlServer2008 = 10;

        private string _providerConnectionString;
        private FreedomDatabaseType _FreedomDatabaseType = FreedomDatabaseType.Offline;

        #region Constructor
        
        public DatabaseBuilderService()
        {
        }

        public DatabaseBuilderService(string providerConnectionString)
        {
            _providerConnectionString = providerConnectionString;
        }

        #endregion

        #region Private Properties

        private string MasterDatabaseConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ProviderConnectionString);
                builder.InitialCatalog = "master";
                builder.AttachDBFilename = string.Empty;
                return builder.ConnectionString;
            }
        }

        private string DatabaseName
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ProviderConnectionString);

                return string.IsNullOrEmpty(builder.InitialCatalog) ? DefaultDatabaseName : builder.InitialCatalog;
            }
        }

        private static int GetSqlServerVersion(SqlConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(connection.ServerVersion) || !Regex.IsMatch(connection.ServerVersion, @"^\d{2}\.\d{2}\.\d{4}$"))
                throw new InvalidOperationException("The Sql Server version number was invalid.");

            return int.Parse(connection.ServerVersion.Substring(0, 2));
        }

        #endregion

        #region Private Methods

        private static string EscapeIdentifier(string identifier)
        {
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        private static async Task SetApplicationSettingAsync(DbConnection connection, string key, string value, CancellationToken cancellationToken)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            DateTime modifyTime = DateTime.UtcNow;

            values.Add("Id", Guid.NewGuid());
            values.Add("CreatedById", User.SuperUserId);
            values.Add("CreatedDateTime", modifyTime);
            values.Add("ModifiedById", User.SuperUserId);
            values.Add("ModifiedDateTime", modifyTime);
            values.Add("Key", key);
            values.Add("Value", value);

            await connection.InsertRecordAsync("ApplicationSetting", values, cancellationToken);
        }

        private static async Task SetDatabaseRevisionAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string revision = version.Revision.ToString(CultureInfo.InvariantCulture);

            if (version.Major == 1)
            {
                Log.Info("Not setting database revision because this is a development build.");
                return;
            }

            Log.Info($"Setting DatabaseRevision to {revision}.");

            await SetApplicationSettingAsync(connection, "DatabaseRevision", revision, cancellationToken);
        }

        private static async Task SetDatabaseGlobalIdentifierAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            Guid globalId = Guid.NewGuid();

            Log.Info($"Assigning this database the global identifier {globalId}.");

            await SetApplicationSettingAsync(connection, "GlobalId", globalId.ToString(), cancellationToken);
        }

        private static async Task CreateAdminUserAndAdminRoleAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            DateTime modifyTime = DateTime.UtcNow;

            Log.InfoFormat("Creating Administrator User and Administrators Role...");

            // Create the Administrator User
            values.Clear();
            values.Add("Id", User.SuperUserId);
            values.Add("Name", "Administrator");
            values.Add("Username", "Administrator");
            values.Add("Password", PasswordUtility.ComputePasswordHash("Freedom"));
            values.Add("IsActive", true);
            values.Add("CreatedById", User.SuperUserId);
            values.Add("CreatedDateTime", modifyTime);
            values.Add("ModifiedById", User.SuperUserId);
            values.Add("ModifiedDateTime", modifyTime);
            values.Add("ForcePasswordChange",false);
            await connection.InsertRecordAsync("User", values, cancellationToken);

            // Create the Administrators Security Role
            values.Clear();
            values.Add("Id", Role.AdministratorsId);
            values.Add("Name", "Administrators");
            values.Add("CreatedById", User.SuperUserId);
            values.Add("CreatedDateTime", modifyTime);
            values.Add("ModifiedById", User.SuperUserId);
            values.Add("ModifiedDateTime", modifyTime);
            await connection.InsertRecordAsync("Role", values, cancellationToken);

            // Add the Administrator to the Administrators Role
            values.Clear();
            values.Add("UserId", User.SuperUserId);
            values.Add("RoleId", Role.AdministratorsId);
            await connection.InsertRecordAsync("UserRole", values, cancellationToken);
        }

        private static async Task CreateIdTableTypeAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            Log.Info("Creating Table Types...");

            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            command.CommandText =
                "CREATE TYPE tvp_IdTable AS TABLE (Id uniqueidentifier NOT NULL, PRIMARY KEY ( Id ));\n" +
                "GRANT EXECUTE ON TYPE::dbo.tvp_IdTable TO public;";

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static async Task CreateServerSequenceTableAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            command.CommandText = "CREATE TABLE _Sequence (" +
                                  "SequenceName nvarchar(64) NOT NULL," +
                                  "NextValue bigint NOT NULL, " +
                                  "BlockSize bigint NOT NULL, " +
                                  "CONSTRAINT PK_Sequence PRIMARY KEY ( SequenceName ))";

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public string ProviderConnectionString
        {
            get { return _providerConnectionString ?? ConfigurationManager.ConnectionStrings["default"].ConnectionString; }
            set
            {
                if (value == _providerConnectionString) return;
                _providerConnectionString = value;
                OnPropertyChanged();
            }
        }

        public FreedomDatabaseType FreedomDatabaseType
        {
            get { return _FreedomDatabaseType; }
            set
            {
                if (_FreedomDatabaseType == value) return;
                _FreedomDatabaseType = value;
                OnPropertyChanged();
            }
        }

        public bool DatabaseExists
        {
            get
            {
                int count;

                using (SqlConnection connection = new SqlConnection(MasterDatabaseConnectionString))
                {
                    connection.Open();

                    DbCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT Count(*) FROM sys.databases WHERE [name] = @databaseName";

                    DbParameter parameter = command.CreateParameter();
                    parameter.Direction = ParameterDirection.Input;
                    parameter.ParameterName = "databaseName";
                    parameter.Value = DatabaseName;
                    command.Parameters.Add(parameter);

                    count = (int) command.ExecuteScalar();
                }

                return count > 0;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public async Task DeleteDatabaseAsync(CancellationToken cancellationToken)
        {
            Log.Info($"Deleting database '{DatabaseName}'...");

            using (SqlConnection masterConnection = new SqlConnection(MasterDatabaseConnectionString))
            {
                string databaseIdentifier = EscapeIdentifier(DatabaseName);

                await masterConnection.OpenAsync(cancellationToken);

                DbCommand command = masterConnection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"alter database {databaseIdentifier} set single_user with rollback immediate";
                await command.ExecuteNonQueryAsync(cancellationToken);

                command = masterConnection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"drop database {databaseIdentifier}";
                await command.ExecuteNonQueryAsync(cancellationToken);

                Log.Info($"Database '{DatabaseName}' deleted.");
            }

            SqlConnection.ClearAllPools();

            OnPropertyChanged(nameof(DatabaseExists));
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public async Task CreateDatabaseAsync(CancellationToken cancellationToken)
        {
            if (DatabaseExists) return;

            // Create the database...

            using (SqlConnection masterConnection = new SqlConnection(MasterDatabaseConnectionString))
            {
                try
                {
                    await masterConnection.OpenAsync(cancellationToken);
                }
                catch (SqlException exception)
                {
                    throw new FreedomDatabaseException(FreedomDatabaseErrorCode.DatabaseConnectionFailed, exception);
                }

                if (GetSqlServerVersion(masterConnection) < SqlServer2008)
                    throw new FreedomDatabaseException(FreedomDatabaseErrorCode.UnsupportedDatabaseVersion);

                try
                {
                    Log.Info($"Creating database '{DatabaseName}'...");

                    string databaseIdentifier = EscapeIdentifier(DatabaseName);

                    await masterConnection.ExecuteNonQueryAsync(
                        $"create database {databaseIdentifier}", cancellationToken);

                    await masterConnection.ExecuteNonQueryAsync(
                        $"alter database {databaseIdentifier} set recovery simple", cancellationToken);

                    Log.Info($"Database '{DatabaseName}' created.");
                }
                catch (SqlException exception)
                {
                    throw new FreedomDatabaseException(FreedomDatabaseErrorCode.UnableToCreateDatabase, exception);
                }
            }

            // Create objects in the database...

            using (SqlConnection connection = new SqlConnection(ProviderConnectionString))
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);
                }
                catch (SqlException exception)
                {
                    throw new FreedomDatabaseException(FreedomDatabaseErrorCode.DatabaseConnectionFailed, exception);
                }

                Log.Info("Creating database objects...");
                
                await CreateServerSequenceTableAsync(connection, cancellationToken);
                await CreateIdTableTypeAsync(connection, cancellationToken);

                await IndexRepository.InitializeAsync(connection, cancellationToken);

                List<string> scripts = new List<string>();
                scripts.Add(FreedomModelResources.CreateDatabaseObjectsScript);                               
                scripts.Add(FreedomModelResources.CreateServerDatabaseObjectsScript);
                
                List<string> batches = string.Join("\r\nGO\r\n", scripts)
                    .Split(new[] {"\r\nGO\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                int i = 0;

                foreach (string batch in batches)
                {
                    string commandText = batch;

                    Log.Info($"Creating objects batch {++i} of {batches.Count}...");

                    try
                    {
                        await connection.ExecuteNonQueryAsync(commandText, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Log.Info(commandText, ex);

                        throw;
                    }
                }

                await CreateAdminUserAndAdminRoleAsync(connection, cancellationToken);
                await SetDatabaseGlobalIdentifierAsync(connection, cancellationToken);
                await SetDatabaseRevisionAsync(connection, cancellationToken);

                Log.Info("The Freedom database has been created and is ready to use.");
            }

            OnPropertyChanged(nameof(DatabaseExists));
        }
    }
}
