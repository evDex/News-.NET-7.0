using System;
using System.Security.Cryptography;
using System.Text;

namespace News.Infrastructure.Common
{
    public static class HashHelper
    {
        public static string Hashing(string text)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return hash;
            }
        }
    }
}
