using System;
using System.Reflection;
using Freedom.Domain.Services.Security;
using log4net;

namespace Freedom.WebApi.Infrastructure
{
    public class FreedomUser
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Guid Id { get; internal set; }

        public bool IsAdministrator { get; internal set; }

        public string DisplayName { get; internal set; }

        public string Domain { get; internal set; }

        public string UserName { get; internal set; }

        public string PasswordHash { get; internal set; }

        public string SymmetricKey { get; internal set; }

        public bool ForcePasswordChange { get; internal set; }

        public FreedomPermissionSet Permissions { get; internal set; }

        internal DateTime CachedDateTime { get; set; }

        public byte[] GetSymmetricKeyBytes()
        {
            if (string.IsNullOrEmpty(SymmetricKey))
                return null;

            try
            {
                byte[] result = Convert.FromBase64String(SymmetricKey);

                if (result.Length != 32)
                    throw new FormatException(
                        $"Expected a 32 byte (256 bit) SymmetricKey but found a {result.Length} key instead.");

                return result;
            }
            catch (FormatException ex)
            {
                Log.Error("Unable to decode the user's SymmetricKey.", ex);

                return null;
            }
        }
    }
}