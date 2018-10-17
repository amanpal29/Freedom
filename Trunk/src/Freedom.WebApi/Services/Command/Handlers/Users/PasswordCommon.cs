
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Freedom.Cryptography;
using Freedom.Extensions;

namespace Freedom.WebApi.Services.Command.Handlers.Users
{
    public static class PasswordCommon
    {
        public static async Task<bool> VerifyPasswordAsync(DbConnection connection, Guid userId, string currentPassword)
        {
            // The authenticaion filter has already validated that the password the user entered when they logged
            // into Hedgehog is still valid.  This is here so that you have to enter your password again when changing
            // it.  This ensures that someone can't change your password if you've left you computer logged in but
            // unattended. - DGG 2011-09-15

            DbCommand dbCommand = connection.CreateCommand();

            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = "select [Password] from [User] where [Id] = @id";

            dbCommand.CreateParameter("id", userId);

            string passwordHash = await dbCommand.ExecuteScalarAsync() as string;

            return PasswordUtility.VerifyPasswordHash(currentPassword, passwordHash);
        }

        public static async Task<bool> ChangePasswordAsync(DbConnection connection, Guid userId, string newPassword, bool forcePasswordChange)
        {
            DbCommand dbCommand = connection.CreateCommand();

            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = "update [User] set [Password]=@password, [ForcePasswordChange]=@forcePasswordChange where [Id] = @id";

            dbCommand.CreateParameter("password", PasswordUtility.ComputePasswordHash(newPassword));
            dbCommand.CreateParameter("forcePasswordChange", forcePasswordChange);
            dbCommand.CreateParameter("id", userId);

            return await dbCommand.ExecuteNonQueryAsync() > 0;
        }
    }
}