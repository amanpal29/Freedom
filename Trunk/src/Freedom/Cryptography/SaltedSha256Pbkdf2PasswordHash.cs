using System.Security.Cryptography;

namespace Freedom.Cryptography
{
    // Password Function Sha256(Pbdkf2(password, salt, 8192 iterations, 32 bytes output))
    // Salt is 256 bits (32 bytes)
    // Recommended for all passwords as of 2016, Iterations should be increased as CPU increases
    public class SaltedSha256Pbkdf2PasswordHash : PasswordHash
    {
        protected int Iterations => 8192;

        protected int InternalKeyLengthInBytes => HashLengthInBytes;

        public override int HashLengthInBytes => 32;

        protected override byte[] DeriveBytesFromPassword(string password, byte[] salt)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
                return pbkdf2.GetBytes(InternalKeyLengthInBytes);
        }

        protected override byte[] HashPasswordBytes(byte[] passwordBytes)
        {
            using (SHA256 sha256 = new SHA256Managed())
                return sha256.ComputeHash(passwordBytes);
        }
    }
}
