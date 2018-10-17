using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Services.Security;
using Freedom.Cryptography;
using Freedom.Extensions;
using log4net;

namespace Freedom.Domain.Services.Repository.PrincipalCache
{
    public static class PrincipalRepository
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int Iterations = 4096;
        private const int BitsPerByte = 8;
        private const int KeySizeInBits = 256;
        private const int KeySizeInBytes = KeySizeInBits/BitsPerByte;
        private const int SaltSizeInBytes = KeySizeInBytes;

        #endregion

        #region Public Methods

        public static async Task InitializeAsync(DbConnection dbConnection, CancellationToken cancellationToken)
        {
            DbCommand command = dbConnection.CreateCommand();

            command.CommandType = CommandType.Text;

            command.CommandText = "CREATE TABLE [Principal] (" +
                                  "[UserId] uniqueidentifier NOT NULL CONSTRAINT [PK_Principal] PRIMARY KEY," +
                                  "[PasswordHash] nvarchar(4000), " +
                                  "[EncryptedPrincipal] varbinary(max) NOT NULL)";

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public static async Task CommitAsync(DbConnection connection, IPrincipal principal)
        {
            FreedomPrincipal FreedomPrincipal = principal as FreedomPrincipal;

            if (FreedomPrincipal != null)
            {
                await CommitAsync(connection, FreedomPrincipal);
            }
        }

        public static async Task CommitAsync(DbConnection connection, FreedomPrincipal principal)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            if (principal.Identity == null)
                throw new ArgumentException("principal must have an identity", nameof(principal));

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            command.CommandText =
                "MERGE[Principal] t USING (VALUES(@userId)) as s ([UserId]) ON t.[UserId] = s.[UserId] " +
                "WHEN MATCHED THEN UPDATE SET t.[PasswordHash] = @passwordHash, t.[EncryptedPrincipal] = @encryptedPrincipal " +
                "WHEN NOT MATCHED THEN INSERT([UserId], [PasswordHash], [EncryptedPrincipal]) VALUES(@userId, @passwordHash, @encryptedPrincipal);";

            command.CreateParameter("userId", principal.UserId);

            FreedomCredentials FreedomCredentials = principal.Identity as FreedomCredentials;

            if (FreedomCredentials != null)
            {
                string passwordHash = PasswordUtility.ComputePasswordHash(FreedomCredentials.Password);
                command.CreateParameter("passwordHash", passwordHash);
            }
            else
            {
                command.CreateParameter("passwordHash", DBNull.Value);
            }

            command.CreateParameter("encryptedPrincipal", EncryptPrincipal(principal));

            if (await command.ExecuteNonQueryAsync() != 1)
                throw new InvalidOperationException("Expected exactly one record to change.");
        }

        public static async Task<FreedomPrincipal> GetPrincipalAsync(DbConnection connection, IIdentity identity)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (identity == null)
                return null;

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                Guid? userId = await GetUserId(connection, identity);

                if (userId == null)
                    return null;

                FreedomCredentials FreedomCredentials = identity as FreedomCredentials;

                DbCommand command = connection.CreateCommand();

                command.CommandType = CommandType.Text;

                command.CommandText =
                    "select [PasswordHash], datalength([EncryptedPrincipal]), [EncryptedPrincipal] " +
                    "from [Principal] where [UserId] = @userId";

                command.CreateParameter("userId", userId);

                using (DbDataReader reader =
                    await command.ExecuteReaderAsync(CommandBehavior.SingleRow | CommandBehavior.SequentialAccess))
                {
                    if (!await reader.ReadAsync())
                        return null;

                    string passwordHash = reader.GetNullableString(0);

                    if (FreedomCredentials != null)
                    {
                        if (!await Task.Run(() => PasswordUtility.VerifyPasswordHash(FreedomCredentials.Password, passwordHash)))
                            return null;
                    }

                    long encryptedPrincipalSize = reader.GetInt64(1);

                    byte[] encryptedPrincipal = new byte[encryptedPrincipalSize];

                    reader.GetBytes(2, 0, encryptedPrincipal, 0, encryptedPrincipal.Length);

                    return DecryptPrincipal(identity, encryptedPrincipal);
                }
            }
            catch (Exception ex)
            {
                Log.Warn(
                    $"An unexpected error occurred while trying to get a cached principal for {identity.AuthenticationType} {identity.Name}.",
                    ex);
            }

            return null;
        }

        public static async Task<Guid?> GetUserId(DbConnection connection, IIdentity identity)
        {
            if (connection == null)
                throw  new ArgumentNullException(nameof(connection));

            if (identity == null)
                return null;

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                DbCommand command = connection.CreateCommand();

                command.CommandType = CommandType.Text;

                WindowsIdentity windowsIdentity = identity as WindowsIdentity;

                if (windowsIdentity != null)
                {
                    command.CommandText =
                        "select [Id] from [User] where [Domain] = @domain and [UserName] = @userName and IsActive = 1";

                    int slashIndex = windowsIdentity.Name.IndexOf('\\');

                    command.CreateParameter("domain", identity.Name.Substring(0, slashIndex));
                    command.CreateParameter("userName", identity.Name.Substring(slashIndex + 1));

                    return await command.ExecuteScalarAsync() as Guid?;
                }

                FreedomCredentials FreedomCredentials = identity as FreedomCredentials;

                if (FreedomCredentials != null)
                {
                    command.CommandText =
                        "select [Id] from [User] where [Domain] is null and [UserName] = @userName and IsActive = 1";

                    command.CreateParameter("userName", FreedomCredentials.Name);

                    return await command.ExecuteScalarAsync() as Guid?;
                }

                Log.Warn($"Don't know how to get the user id for identity {identity.Name} ({identity.AuthenticationType}).");
            }
            catch (Exception ex)
            {
                Log.Warn($"An unexpected error occurred while trying to get the userId for {identity.AuthenticationType} {identity.Name}.", ex);
            }
            
            return null;
        }

        #endregion

        #region Private Helpers

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static byte[] EncryptPrincipal(FreedomPrincipal principal)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(FreedomPrincipal));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                ICryptoTransform encryptor = GetEncryptor(principal.Identity, memoryStream);

                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    serializer.WriteObject(cryptoStream, principal);

                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static FreedomPrincipal DecryptPrincipal(IIdentity identity, byte[] encryptedPrincipal)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(FreedomPrincipal));

            using (MemoryStream memoryStream = new MemoryStream(encryptedPrincipal))
            {
                ICryptoTransform decryptor = GetDecryptor(identity, memoryStream);

                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    FreedomPrincipal principal = (FreedomPrincipal)serializer.ReadObject(cryptoStream);

                    return new FreedomPrincipal(identity, principal);
                }
            }
        }

        private static ICryptoTransform GetEncryptor(IIdentity identity, Stream stream)
        {
            SymmetricAlgorithm algorithm = new AesCryptoServiceProvider();

            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

            byte[] salt = new byte[SaltSizeInBytes];

            randomNumberGenerator.GetBytes(salt);

            stream.Write(salt, 0, salt.Length);

            byte[] iv = new byte[algorithm.BlockSize / BitsPerByte];

            randomNumberGenerator.GetBytes(iv);

            stream.Write(iv, 0, iv.Length);

            algorithm.KeySize = KeySizeInBits;
            algorithm.Key = GenerateKey(salt, identity);
            algorithm.IV = iv;

            return algorithm.CreateEncryptor();
        }

        private static ICryptoTransform GetDecryptor(IIdentity identity, Stream stream)
        {
            SymmetricAlgorithm algorithm = new AesCryptoServiceProvider();

            byte[] salt = new byte[SaltSizeInBytes];

            stream.Read(salt, 0, salt.Length);

            byte[] iv = new byte[algorithm.BlockSize / BitsPerByte];

            stream.Read(iv, 0, iv.Length);

            algorithm.KeySize = KeySizeInBits;
            algorithm.Key = GenerateKey(salt, identity);
            algorithm.IV = iv;

            return algorithm.CreateDecryptor();
        }

        private static byte[] GenerateKey(byte[] salt, IIdentity identity)
        {
            if (salt == null)
                throw new ArgumentNullException(nameof(salt));

            if (salt.Length == 0 || salt.All(b => b == 0x00))
                throw new ArgumentException("salt cannot be null, empty, or all zeros.", nameof(salt));

            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            WindowsIdentity windowsIdentity = identity as WindowsIdentity;

            if (windowsIdentity?.Owner != null)
            {
                byte[] passwordBytes = new byte[windowsIdentity.Owner.BinaryLength];

                windowsIdentity.Owner.GetBinaryForm(passwordBytes, 0);

                using (DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, salt, Iterations))
                    return key.GetBytes(KeySizeInBytes);
            }

            FreedomCredentials FreedomCredentials = identity as FreedomCredentials;

            if (FreedomCredentials != null)
            {
                using (DeriveBytes key = new Rfc2898DeriveBytes(FreedomCredentials.Password, salt, Iterations))
                    return key.GetBytes(KeySizeInBytes);
            }

            throw new InvalidOperationException("The current identity is not supported.");
        }

        #endregion
    }
}
