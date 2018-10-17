using System;
using System.Security.Cryptography;
using System.Text;

namespace Freedom.Cryptography
{
    // Password function: base64(MD5(salt + UTF8(base64(MD5(UTF8(password))))))
    // Salt is 64 bits (8 bytes)
    // This was the algorithm used in Freedom for the offline database for all 4.x versions
    public class Md5SaltedMd5PasswordHash : PasswordHash
    {
        public override int SaltLengthInBytes => 8;

        public override int HashLengthInBytes => 16;

        protected override byte[] DeriveBytesFromPassword(string password, byte[] salt)
        {
            byte[] firstHash = HashPasswordBytes(Encoding.UTF8.GetBytes(password));

            byte[] base64 = Encoding.UTF8.GetBytes(Convert.ToBase64String(firstHash));

            return Concatenate(salt, base64);
        }

        protected override byte[] HashPasswordBytes(byte[] passwordBytes)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
                return md5.ComputeHash(passwordBytes);
        }
    }
}