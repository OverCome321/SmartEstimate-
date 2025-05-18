using Sodium;
using System.IO;
using System.Text;

namespace SmartEstimateApp.Manager
{
    public class CredentialsManager
    {
        private readonly Action<string> _showError;

        public CredentialsManager(Action<string> showError = null)
        {
            _showError = showError;
        }

        public void SaveCredentials(string email, string password, bool rememberMe)
        {
            try
            {
                string path = GetCredentialsFilePath();
                if (rememberMe)
                {
                    string credentials = $"{email}:{password}:{DateTime.UtcNow.AddDays(30).Ticks}";
                    byte[] data = Encoding.UTF8.GetBytes(credentials);
                    byte[] key = GenerateOrLoadKey();
                    byte[] nonce = SodiumCore.GetRandomBytes(24);
                    byte[] encryptedData = SecretBox.Create(data, nonce, key);

                    byte[] combinedData = new byte[nonce.Length + encryptedData.Length];
                    Buffer.BlockCopy(nonce, 0, combinedData, 0, nonce.Length);
                    Buffer.BlockCopy(encryptedData, 0, combinedData, nonce.Length, encryptedData.Length);

                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, combinedData);
                }
                else
                {
                    ClearCredentials();
                }
            }
            catch (Exception ex)
            {
                _showError?.Invoke($"Ошибка при сохранении учетных данных: {ex.Message}");
            }
        }

        public (string Email, string Password, bool IsValid) LoadCredentials()
        {
            try
            {
                string path = GetCredentialsFilePath();
                if (File.Exists(path))
                {
                    byte[] combinedData = File.ReadAllBytes(path);
                    byte[] nonce = new byte[24];
                    byte[] encryptedData = new byte[combinedData.Length - nonce.Length];

                    Buffer.BlockCopy(combinedData, 0, nonce, 0, nonce.Length);
                    Buffer.BlockCopy(combinedData, nonce.Length, encryptedData, 0, encryptedData.Length);

                    byte[] key = GenerateOrLoadKey();
                    byte[] decryptedData = SecretBox.Open(encryptedData, nonce, key);
                    string credentials = Encoding.UTF8.GetString(decryptedData);
                    var parts = credentials.Split(':');

                    if (parts.Length != 3)
                    {
                        ClearCredentials();
                        return (null, null, false);
                    }

                    if (long.TryParse(parts[2], out long ticks))
                    {
                        var expiration = new DateTime(ticks);
                        if (DateTime.UtcNow > expiration)
                        {
                            ClearCredentials();
                            return (null, null, false);
                        }
                    }

                    return (parts[0], parts[1], true);
                }
            }
            catch (Exception)
            {
                ClearCredentials();
            }
            return (null, null, false);
        }

        public void ClearCredentials()
        {
            try
            {
                string path = GetCredentialsFilePath();
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _showError?.Invoke($"Ошибка при очистке учетных данных: {ex.Message}");
            }
        }

        private string GetCredentialsFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartEstimate", "credentials.dat");
        }

        private byte[] GenerateOrLoadKey()
        {
            string keyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartEstimate", "key.dat");
            if (File.Exists(keyPath))
            {
                return File.ReadAllBytes(keyPath);
            }
            byte[] key = SodiumCore.GetRandomBytes(32);
            Directory.CreateDirectory(Path.GetDirectoryName(keyPath));
            File.WriteAllBytes(keyPath, key);
            return key;
        }
    }
}
