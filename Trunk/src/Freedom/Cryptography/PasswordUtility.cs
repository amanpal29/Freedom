using System;
using System.Collections.Generic;
using System.Linq;

namespace Freedom.Cryptography
{
    public static class PasswordUtility
    {
        private static List<IPasswordHash> PasswordHashes { get; }

        static PasswordUtility()
        {
            PasswordHashes = new List<IPasswordHash>();

            PasswordHashes.Add(new SaltedSha256Pbkdf2PasswordHash());
            PasswordHashes.Add(new Md5PasswordHash());
            PasswordHashes.Add(new Md5SaltedMd5PasswordHash());
            PasswordHashes.Add(new PlainTextPasswordHash());
        }

        public static string ComputePasswordHash(string password)
        {
            if (string.IsNullOrEmpty(password))
                return null;

            return PasswordHashes[0].ComputePasswordHash(password);
        }

        public static bool VerifyPasswordHash(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
                return false;

            return PasswordHashes.Any(hash => hash.VerifyPasswordHash(password, passwordHash));
        }

        [Flags]
        private enum CharacterType
        {
            None = 0,
            Lower = 0x01,
            Upper = 0x02,
            Digit = 0x04,
            Punctuation = 0x08,
            Other = 0x10
        }

        private static CharacterType GetCharacterType(char c)
        {
            if (char.IsLower(c))
                return CharacterType.Lower;

            if (char.IsUpper(c))
                return CharacterType.Upper;

            if (char.IsDigit(c))
                return CharacterType.Digit;

            if (char.IsPunctuation(c))
                return CharacterType.Punctuation;

            return CharacterType.Other;
        }

        public static int GetPasswordComplexity(IEnumerable<char> password)
        {
            int result = 0;

            CharacterType characterTypes = CharacterType.None;

            foreach (char c in password)
            {
                CharacterType characterType = GetCharacterType(c);

                if (characterTypes.HasFlag(characterType)) continue;

                characterTypes |= characterType;

                result++;
            }

            return result;
        }
    }
}
