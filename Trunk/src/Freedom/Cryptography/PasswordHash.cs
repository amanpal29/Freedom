using System;
using System.Collections;
using System.Security.Cryptography;

namespace Freedom.Cryptography
{
    public abstract class PasswordHash : IPasswordHash
    {
        public virtual int SaltLengthInBytes => HashLengthInBytes;

        public abstract int HashLengthInBytes { get; }

        protected static byte[] Concatenate(byte[] x, byte[] y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));

            if (y == null)
                throw new ArgumentNullException(nameof(y));

            byte[] result = new byte[x.Length + y.Length];

            x.CopyTo(result, 0);
            y.CopyTo(result, x.Length);

            return result;
        }

        protected static byte[] GenerateRandomSalt(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "length must be >= 0");

            if (length == 0)
                return null;

            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

            byte[] salt = new byte[length];

            randomNumberGenerator.GetBytes(salt);

            return salt;
        }

        protected abstract byte[] DeriveBytesFromPassword(string password, byte[] salt);

        protected abstract byte[] HashPasswordBytes(byte[] passwordBytes);

        public string ComputePasswordHash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            byte[] salt = GenerateRandomSalt(SaltLengthInBytes);

            byte[] passwordBytes = DeriveBytesFromPassword(password, salt);

            byte[] hashBytes = HashPasswordBytes(passwordBytes);

            return Convert.ToBase64String(salt != null ? Concatenate(salt, hashBytes) : hashBytes);
        }

        public bool VerifyPasswordHash(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrEmpty(passwordHash))
                return false;

            try
            {
                int expectedStringLength = (SaltLengthInBytes + HashLengthInBytes + 2) / 3 * 4;

                if (passwordHash.Length != expectedStringLength)
                    return false;

                byte[] saltAndHashBytes = Convert.FromBase64String(passwordHash);

                if (saltAndHashBytes.Length != SaltLengthInBytes + HashLengthInBytes)
                    return false;

                byte[] salt = new byte[SaltLengthInBytes];

                Array.Copy(saltAndHashBytes, 0, salt, 0, salt.Length);

                byte[] storedHashBytes = new byte[HashLengthInBytes];

                Array.Copy(saltAndHashBytes, salt.Length, storedHashBytes, 0, storedHashBytes.Length);

                byte[] passwordBytes = DeriveBytesFromPassword(password, salt);

                byte[] computedHashBytes = HashPasswordBytes(passwordBytes);

                return StructuralComparisons.StructuralEqualityComparer.Equals(storedHashBytes, computedHashBytes);
            }
            catch (FormatException)
            {
                // passwordHash was not a valid base64 string
                return false;
            }
        }
    }
}