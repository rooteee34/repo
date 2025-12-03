using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SystemManager.Modules.PasswordManager
{
    public class PasswordGenerator
    {
        private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string Special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        public string GeneratePassword(int length = 16, bool includeSpecial = true, bool includeNumbers = true, bool includeUpperCase = true)
        {
            var charPool = LowerCase;
            if (includeUpperCase) charPool += UpperCase;
            if (includeNumbers) charPool += Digits;
            if (includeSpecial) charPool += Special;

            using (var rng = new RNGCryptoServiceProvider())
            {
                var data = new byte[length];
                rng.GetBytes(data);
                
                var result = new StringBuilder(length);
                foreach (var b in data)
                {
                    result.Append(charPool[b % charPool.Length]);
                }
                return result.ToString();
            }
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
