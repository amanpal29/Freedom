using System.Security.Cryptography;
using System.Text;

namespace Freedom.Cryptography
{
    // Password function: MD5(UTF8(password))
    // This algorithm doesn't use any salt, and is therefore weak as fuck.
    // This was the algorithm used in Freedom for the server databases in version 4.x
    public class Md5PasswordHash : PasswordHash
    {
        public override int SaltLengthInBytes => 0;

        public override int HashLengthInBytes => 16;

        protected override byte[] DeriveBytesFromPassword(string password, byte[] salt)
        {
            return Encoding.UTF8.GetBytes(password);
        }

        protected override byte[] HashPasswordBytes(byte[] passwordBytes)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
                return md5.ComputeHash(passwordBytes);
        }
    }
}
