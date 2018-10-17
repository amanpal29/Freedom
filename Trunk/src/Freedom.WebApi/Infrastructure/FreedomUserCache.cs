using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Security;
using Freedom.Extensions;
using log4net;

namespace Freedom.WebApi.Infrastructure
{
    public class FreedomUserCache : IFreedomUserCache
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int BitsPerByte = 8;
        private const int SymmetricKeySizeInBits = 256;
        private const int SymmetricKeySizeInBytes = SymmetricKeySizeInBits / BitsPerByte;

        private readonly ConcurrentDictionary<string, FreedomUser> _cache =
            new ConcurrentDictionary<string, FreedomUser>(StringComparer.OrdinalIgnoreCase);

        private TimeSpan _maxCacheAge = new TimeSpan(0, 2, 0);

        public TimeSpan MaxCacheAge
        {
            get { return _maxCacheAge; }
            set { _maxCacheAge = value < TimeSpan.Zero ? TimeSpan.MaxValue : value; }
        }

        public FreedomUser GetUserDataFromCache(string name)
        {
            FreedomUser user;

            if (!_cache.TryGetValue(name, out user))
                return null;

            TimeSpan age = DateTime.UtcNow - user.CachedDateTime;

            return age < MaxCacheAge ? user : null;
        }

        public async Task<FreedomUser> GetFreedomUserFromDatabaseAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string domain, userName;

            int slashIndex = name.IndexOf('\\');

            if (slashIndex < 0)
            {
                domain = null;
                userName = name;
            }
            else
            {
                domain = name.Substring(0, slashIndex);
                userName = name.Substring(slashIndex + 1);
            }

            try
            {
                using (DbConnection connection = IoC.Get<DbConnection>())
                {
                    if (connection.State != ConnectionState.Open)
                        await connection.OpenAsync();

                    FreedomUser freedomUser = await LoadUserInformationAsync(connection, domain, userName);

                    if (freedomUser == null)
                        return null;

                    freedomUser.Permissions = await LoadUserPermissionsAsync(connection, freedomUser.Id);

                    _cache[name] = freedomUser;

                    return freedomUser;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.Warn($"An exception occurred while attempting to load user '{domain}\\{userName}' from the database.", ex);
            }
            catch (DbException ex)
            {
                Log.Warn($"An exception occurred while attempting to load user '{domain}\\{userName}' from the database.", ex);
            }

            return null;
        }

        private static string GenerateNewSymmetricKey()
        {
            byte[] key = new byte[SymmetricKeySizeInBytes];

            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

            randomNumberGenerator.GetBytes(key);

            return Convert.ToBase64String(key, Base64FormattingOptions.None);
        }

        private static async Task InitializeSymmetricKeyAsync(DbConnection connection, FreedomUser user)
        {
            string newKey = GenerateNewSymmetricKey();

            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = "update [User] set SymmetricKey=@symmetricKey where [Id]=@id and SymmetricKey is null";

            command.CreateParameter("symmetricKey", newKey);
            command.CreateParameter("id", user.Id);

            if (await command.ExecuteNonQueryAsync() > 0)
            {
                user.SymmetricKey = newKey;
                return;
            }

            // This handles the unlikely but possible race condidion of the SymmetricKey having just been set in another thread.
            command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "select [SymmetricKey] from [User] where [Id]=@id";
            command.CreateParameter("id", user.Id);
            user.SymmetricKey = await command.ExecuteScalarAsync() as string;
        }

        private static async Task<FreedomUser> LoadUserInformationAsync(DbConnection connection, string domain, string userName)
        {
            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            command.CreateParameter("adminsId", Role.AdministratorsId);

            if (string.IsNullOrEmpty(domain))
            {
                command.CommandText =
                    "SELECT u.Id, u.Name, u.UserName, u.Password, u.SymmetricKey, u.Domain, u.ForcePasswordChange, "
                    + "CASE WHEN ur.UserId IS NULL THEN 0 ELSE 1 END [IsAdministrator] "
                    + "FROM [User] u LEFT JOIN [UserRole] ur ON u.Id = ur.UserId AND ur.RoleId = @adminsId "
                    + "WHERE (u.Domain IS NULL) AND (u.UserName = @username) AND u.IsActive = 1";
            }
            else
            {
                command.CommandText =
                    "SELECT u.Id, u.Name, u.UserName, u.Password, u.SymmetricKey, u.Domain, u.ForcePasswordChange, "
                    + "CASE WHEN ur.UserId IS NULL THEN 0 ELSE 1 END [IsAdministrator] "
                    + "FROM [User] u LEFT JOIN [UserRole] ur ON u.Id = ur.UserId AND ur.RoleId = @adminsId "
                    + "WHERE (u.Domain = @domain) AND (u.UserName = @username) AND u.IsActive = 1";

                command.CreateParameter("domain", domain);
            }

            command.CreateParameter("username", userName);

            using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
            {
                if (!reader.Read())
                    return null;

                FreedomUser user = new FreedomUser();

                user.Id = reader.GetGuid(0);
                user.DisplayName = reader.GetNullableString(1);
                user.UserName = reader.GetNullableString(2);
                user.PasswordHash = reader.GetNullableString(3);
                user.SymmetricKey = reader.GetNullableString(4);
                user.Domain = reader.GetNullableString(5);
                user.ForcePasswordChange = reader.GetBoolean(6);
                user.IsAdministrator = reader.GetInt32(7) == 1;
                user.CachedDateTime = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(user.DisplayName))
                    user.DisplayName = user.UserName;

                reader.Close();

                if (string.IsNullOrEmpty(user.SymmetricKey))
                    await InitializeSymmetricKeyAsync(connection, user);

                return user;
            }
        }

        private static async Task<FreedomPermissionSet> LoadUserPermissionsAsync(DbConnection connection, Guid userId)
        {
            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            command.CommandText = "select distinct p.[Description] " +
                                  "from [Permission] p join [UserRole] ur on p.[RoleId] = ur.[RoleId] " +
                                  "where ur.[UserId] = @userId";

            command.CreateParameter("userId", userId);

            FreedomPermissionSet permissionSet = new FreedomPermissionSet();

            using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
            {
                while (await reader.ReadAsync())
                {
                    if (await reader.IsDBNullAsync(0))
                        continue;

                    permissionSet.Add(reader.GetString(0));
                }
            }

            return permissionSet;
        }
    }
}