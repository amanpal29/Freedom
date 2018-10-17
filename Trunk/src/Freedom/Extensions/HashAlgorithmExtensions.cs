using System;
using System.Security.Cryptography;
using System.Text;

namespace Freedom.Extensions
{
    public static class HashAlgorithmExtensions
    {
        public static string ComputeHashString(this HashAlgorithm hashAlgorithm, string input)
        {
            return Convert.ToBase64String(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }
    }
}
