using System.Security.Cryptography;

namespace DatabaseController
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(4);
            int iterations = 100;
            var key = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var result = Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(key.GetBytes(16));
            return result;
        }

        public static async Task<string> HashPasswordAsync(string password)
            => await Task.Run(() => HashPassword(password));

        public static bool ArePasswordsEqual(string password, string hashPassword)
        {
            if (password == null) return false;
            var sepInd = hashPassword.IndexOf(':');
            var salt = Convert.FromBase64String(hashPassword.Substring(0, sepInd));
            int iterations = 100;
            var key = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var result = Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(key.GetBytes(16));
            return hashPassword == result;
        }

        public static async Task<bool> ArePasswordsEqualAsync(string password, string hashPassword)
            => await Task.Run(() => ArePasswordsEqual(password, hashPassword));
    }
}
