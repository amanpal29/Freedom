using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Web.Http;
using Freedom.WebApi.Filters;
using Freedom.WebApi.Results;
using Freedom.Domain.Services.Security;

namespace Freedom.WebApi.Controllers
{
    [FreedomAuthentication]
    public class SecurityController : ApiController
    {
        [Route("security/principal")]
        public async Task<IHttpActionResult> GetPrincipal()
        {
            FreedomIdentity hedgehogUser = User?.Identity as FreedomIdentity;

            if (hedgehogUser?.ForcePasswordChange != true)
                return Ok((FreedomPrincipal) User);

            PasswordPolicy passwordPolicy = await GetPasswordPolicyAsync();

            return new PasswordMustBeChangedResult(passwordPolicy, this);
        }

        private static async Task<PasswordPolicy> GetPasswordPolicyAsync()
        {
            PasswordPolicy passwordPolicy = new PasswordPolicy();

            passwordPolicy.MinimumPasswordComplexity = 3;
            passwordPolicy.MinimumPasswordLength = 8;

            using (DbConnection dbConnection = IoC.Get<DbConnection>())
            {
                if (dbConnection.State != ConnectionState.Open)
                    await dbConnection.OpenAsync();

                DbCommand command = dbConnection.CreateCommand();

                command.CommandType = CommandType.Text;
                command.CommandText =
                    "select [Key], [Value] " + 
                    "from [ApplicationSetting] " +
                    "where [Key] in ('MinimumPasswordComplexity', 'MinimumPasswordLength')";

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                {
                    while (await reader.ReadAsync())
                    {
                        int i;

                        string key = !reader.IsDBNull(0) ? reader.GetString(0) : string.Empty;
                        string value = !reader.IsDBNull(1) ? reader.GetString(1) : null;

                        if (!int.TryParse(value, out i) || i < 0) continue;

                        switch (key)
                        {
                            case "MinimumPasswordComplexity":
                                passwordPolicy.MinimumPasswordComplexity = Math.Min(i, 5);
                                break;

                            case "MinimumPasswordLength":
                                passwordPolicy.MinimumPasswordLength = i;
                                break;
                        }
                    }
                }

                return passwordPolicy;
            }
        }
    }
}
