using System;

namespace Freedom.Cryptography
{
    // No crypto at all, never use this in production code.
    public class PlainTextPasswordHash : IPasswordHash
    {
        public string ComputePasswordHash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            return password;
        }

        public bool VerifyPasswordHash(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            return password == passwordHash;
        }
    }
}