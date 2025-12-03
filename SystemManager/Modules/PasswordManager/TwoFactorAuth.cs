using System;
using System.Security.Cryptography;
using System.Text;

namespace SystemManager.Modules.PasswordManager
{
    public class TwoFactorAuth
    {
        public string GenerateTOTPCode(string secret)
        {
            var key = Encoding.UTF8.GetBytes(secret);
            var counter = GetCurrentCounter();
            var hash = ComputeHMAC(key, BitConverter.GetBytes(counter));
            
            var offset = hash[hash.Length - 1] & 0x0F;
            var code = ((hash[offset] & 0x7F) << 24) |
                       ((hash[offset + 1] & 0xFF) << 16) |
                       ((hash[offset + 2] & 0xFF) << 8) |
                       (hash[offset + 3] & 0xFF);
            
            return (code % 1000000).ToString("D6");
        }

        private long GetCurrentCounter()
        {
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return unixTime / 30; // 30-second intervals
        }

        private byte[] ComputeHMAC(byte[] key, byte[] data)
        {
            using (var hmac = new HMACSHA1(key))
            {
                return hmac.ComputeHash(data);
            }
        }
    }
}
