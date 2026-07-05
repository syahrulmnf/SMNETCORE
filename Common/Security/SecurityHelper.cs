using System;
using System.Text;
using System.Security.Cryptography;

namespace SMNETCORE.Common.Security
{
   public class SecurityHelper
    {
        private static string SaltAddition = "FbASAP";

        /// <summary>
        /// Hashes a password with the additional Salt text
        /// </summary>
        /// <param name="password">user's password</param>
        /// <param name="passwordSalt">salt</param>
        /// <returns>hash of password</returns>
        public static string HashPassword(string password, string passwordSalt)
        {
            return HashString(string.Format("{0}{1}{2}", passwordSalt, password, SaltAddition));
        }

        /// <summary>
        /// Hashes a string using a specified algorithm.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="hashName">Name of the hashing algorithm to use.</param>
        /// <returns>Hash as a Base64 string.</returns>
        public static string HashString(string inputString, string hashName = "SHA256")
        {
            HashAlgorithm algorithm = HashAlgorithm.Create(hashName);
            if (algorithm == null)
            {
                throw new ArgumentException("Unrecognized hash name", "hashName");
            }
            byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Generates a random un-ambiguous password
        /// </summary>
        /// <param name="length">Requested length</param>
        /// <returns>Password</returns>
        public static string GeneratePassword(int length = 12)
        {
            return RandomPassword.Generate(length);
        }

        /// <summary>
        /// Checks a password is correct
        /// </summary>
        /// <param name="password">User entered password</param>
        /// <param name="passwordSalt">User stored SALT</param>
        /// <param name="passwordHash">User stored Password Hash</param>
        /// <returns>true/false</returns>
        public static bool AuthenticatePassword(string password, string passwordSalt, string passwordHash)
        {
            // check password
            if (!String.IsNullOrEmpty(passwordSalt))
            {
                string hashPWD = HashPassword(password, passwordSalt);
                return (hashPWD == passwordHash);
            }
            else
            {

                return (password == passwordHash);
            }
        }

        public static bool IsDefaultPassword(string password, string defaultPassword, string passwordSalt, string passwordHash)
        {
            string defaultHashPWD = HashPassword(defaultPassword, passwordSalt);
            if (!String.IsNullOrEmpty(passwordSalt))
            {
                string hashPWD = HashPassword(password, passwordSalt);

                return (hashPWD == defaultHashPWD);
            }
            return (defaultHashPWD == passwordHash);
        }
    }
}
