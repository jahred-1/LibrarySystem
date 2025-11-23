using System;
using System.Security.Cryptography;
using System.Text;

namespace LibrarySystem
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashes a password using SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            string hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}


