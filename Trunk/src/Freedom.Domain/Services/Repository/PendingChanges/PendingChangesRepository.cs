using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;
using Freedom.Domain.Services.Security;
using Freedom.Domain.Services.Time;
using Freedom.Extensions;
using log4net;

namespace Freedom.Domain.Services.Repository.PendingChanges
{
    public class PendingChangesRepository
    {
        private const int InitializationVectorSizeInBytes = 16;

        #region SQL Command String Constants

        private const string CreateTableSql =
            "CREATE TABLE [_PendingChanges] (" +
            "[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_PendingChanges] PRIMARY KEY, " +
            "[UserId] uniqueidentifier NOT NULL, " +
            "[TransactionDateTime] datetimeoffset NOT NULL, " +
            "[Payload] varbinary(max) NOT NULL, " +
            "[Attempts] int NOT NULL CONSTRAINT [D_Attempts] DEFAULT (0), " +
            "[LastAttempt] datetimeoffset NULL, " +
            "[LastError] nvarchar(max) NULL)";

        private const string InsertSql = "INSERT INTO [_PendingChanges] ([UserId], [TransactionDateTime], [Payload]) " +
                                         "VALUES (@userid, @transactionDateTime, @payload) " +
                                         "SET @id = SCOPE_IDENTITY()";

        private const string RemoveSql = "DELETE FROM [_PendingChanges] WHERE [Id] = @id";

        private const string MarkFailedAttemptSql = "UPDATE [_PendingChanges] " +
                                                    "SET [Attempts] = [Attempts] + 1, [LastAttempt] = @now, [LastError] = @error " +
                                                    "WHERE [Id] = @id";

        #endregion

        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ITimeService _timeService = IoC.TryGet<ITimeService>() ?? new LocalTimeService();

        private readonly DbConnection _dbConnection;

        #endregion

        #region Constructor

        public PendingChangesRepository(DbConnection dbConnection)
        {
            if (dbConnection == null)
                throw new ArgumentNullException(nameof(dbConnection));

            _dbConnection = dbConnection;
        }

        #endregion

        #region Initialize Method

        public static async Task InitializeAsync(DbConnection dbConnection, CancellationToken cancellationToken)
        {
            if (dbConnection.State != ConnectionState.Open)
                await dbConnection.OpenAsync(cancellationToken);

            DbCommand command = dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CreateTableSql;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        #endregion

        #region Private Methods

        private async Task EnsureConnectionOpenAsync()
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();
        }

        private static SymmetricAlgorithm GetSymmetricAlgorithm(byte[] symmetricKey)
        {
            if (symmetricKey == null)
                throw new ArgumentNullException(nameof(symmetricKey));

            SymmetricAlgorithm symmetricAlgorithm = new AesCryptoServiceProvider();

            symmetricAlgorithm.KeySize = symmetricKey.Length * 8;
            symmetricAlgorithm.Key = symmetricKey;

            return symmetricAlgorithm;
        }

        private static ICryptoTransform GetEncryptor(byte[] iv, byte[] symmetricKey)
        {
            if (iv == null)
                throw new ArgumentNullException(nameof(iv));

            if (symmetricKey == null)
                throw new ArgumentNullException(nameof(symmetricKey));

            SymmetricAlgorithm symmetricAlgorithm = GetSymmetricAlgorithm(symmetricKey);

            if (iv.Length * 8 != symmetricAlgorithm.BlockSize)
                throw new ArgumentException("iv length must match symmetric encryption block size.", nameof(iv));

            symmetricAlgorithm.IV.CopyTo(iv, 0);

            return symmetricAlgorithm.CreateEncryptor();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static byte[] EncryptPayload(byte[] symmetricKey, CommandBase command)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(CommandBase));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] iv = new byte[InitializationVectorSizeInBytes];

                ICryptoTransform encryptor = GetEncryptor(iv, symmetricKey);

                memoryStream.Write(iv, 0, iv.Length);

                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (Stream compressionStream = new GZipStream(cryptoStream, CompressionMode.Compress, true))
                        serializer.WriteObject(compressionStream, command);

                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
            }
        }

        #endregion

        public async Task<int> CommitAsync(IPrincipal currentPrincipal, CommandBase command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (currentPrincipal == null)
                throw new ArgumentNullException(nameof(currentPrincipal));

            FreedomPrincipal FreedomPrincipal = currentPrincipal as FreedomPrincipal;
            
            if (FreedomPrincipal == null)
                throw new ArgumentException("currentPrincipal must be an instance of FreedomPrincipal", nameof(currentPrincipal));

            if (FreedomPrincipal.SymmetricKey == null || FreedomPrincipal.SymmetricKey.Length == 0)
                throw new ArgumentException("The currentPrincipal does not have a SymmetricKey.", nameof(currentPrincipal));

            await EnsureConnectionOpenAsync();

            DbCommand dbCommand = _dbConnection.CreateCommand();

            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = InsertSql;

            dbCommand.CreateParameter("userId", FreedomPrincipal.UserId);
            dbCommand.CreateParameter("transactionDateTime", _timeService.Now);
            dbCommand.CreateParameter("payload", EncryptPayload(FreedomPrincipal.SymmetricKey, command));

            DbParameter output = dbCommand.CreateParameter();
            output.ParameterName = "id";
            output.Direction = ParameterDirection.Output;
            output.DbType = DbType.Int32;
            dbCommand.Parameters.Add(output);

            await dbCommand.ExecuteNonQueryAsync();

            return (int) output.Value;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            try
            {
                await EnsureConnectionOpenAsync();

                DbCommand dbCommand = _dbConnection.CreateCommand();

                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = RemoveSql;

                dbCommand.CreateParameter("id", id);

                await dbCommand.ExecuteNonQueryAsync();

                return true;
            }
            catch (DbException ex)
            {
                Log.Warn($"An error occurred trying to remove pending change {id} from the offline database.", ex);

                return false;
            }
        }

        public async Task<bool> MarkedFailedAsync(int id, Exception error)
        {
            try
            {
                await EnsureConnectionOpenAsync();

                DbCommand dbCommand = _dbConnection.CreateCommand();

                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = MarkFailedAttemptSql;

                dbCommand.CreateParameter("now", _timeService.Now);
                dbCommand.CreateParameter("error", (object) error?.ToString() ?? DBNull.Value);
                dbCommand.CreateParameter("id", id);

                await dbCommand.ExecuteNonQueryAsync();

                return true;
            }
            catch (DbException ex)
            {
                Log.Warn($"An error occurred trying to remove pending change {id} from the offline database.", ex);

                return false;
            }
        }

        public async Task<int> GetCountAsync(IPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            FreedomPrincipal FreedomPrincipal = principal as FreedomPrincipal;

            if (FreedomPrincipal == null)
                throw new ArgumentException("principal must be an instance of FreedomPrincipal", nameof(principal));

            if (FreedomPrincipal.SymmetricKey == null || FreedomPrincipal.SymmetricKey.Length == 0)
                throw new ArgumentException("The principal does not have a SymmetricKey.", nameof(principal));

            try
            {
                await EnsureConnectionOpenAsync();

                DbCommand dbCommand = _dbConnection.CreateCommand();

                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = PendingChangesEnumerable.CountSql;

                dbCommand.CreateParameter("userId", FreedomPrincipal.UserId);

                return (int) await dbCommand.ExecuteScalarAsync();
            }
            catch (DbException ex)
            {
                Log.Warn($"An error occurred tring to get the count of pending changes for user {FreedomPrincipal.UserId}.", ex);

                return 0;
            }
        }

        public IEnumerable<PendingChange> GetPendingChanges(IPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            FreedomPrincipal FreedomPrincipal = principal as FreedomPrincipal;

            if (FreedomPrincipal == null)
                throw new ArgumentException("principal must be an instance of FreedomPrincipal", nameof(principal));

            if (FreedomPrincipal.SymmetricKey == null || FreedomPrincipal.SymmetricKey.Length == 0)
                throw new ArgumentException("The principal does not have a SymmetricKey.", nameof(principal));

            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            return new PendingChangesEnumerable(_dbConnection, FreedomPrincipal.UserId, FreedomPrincipal.SymmetricKey);
        } 
    }
}
