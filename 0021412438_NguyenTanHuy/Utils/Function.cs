using System.Security.Cryptography;
using System.Text;

namespace _0021412438_NguyenTanHuy.Utils
{
    public class Function
    {
        public static string GetMd5Hash(string input)
        {
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashBytes).ToLower();
        }
        public static bool VerifyPasswordHash(string password, string storedHash)
        {
            var hashOfInput = GetMd5Hash(password);
            return hashOfInput.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }

}
