using System.Security.Cryptography;

namespace Common.Security
{
    public static class PasswordHasher
    {
        private const int Iterations = 10000;

        private const int HashSize = 32;

        private const int SaltSize = 16;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(HashSize);

            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return System.Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string storedHash, string password)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password))
                return false;

            try
            {
                byte[] hashBytes = System.Convert.FromBase64String(storedHash);

                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256);

                byte[] hash = pbkdf2.GetBytes(HashSize);

                bool passwordMatch = true;
                for (int i = 0; i < HashSize; i++)
                {
                    passwordMatch &= (hashBytes[i + SaltSize] == hash[i]);
                }

                return passwordMatch;
            }
            catch
            {
                return false;
            }
        }
    }
}