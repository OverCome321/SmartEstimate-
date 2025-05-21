using Sodium;
using System.Text;

namespace OpenAIService.Security
{
    public static class ApiKeyEncryptor
    {
        // Общая папка для наших файлов в LocalAppData
        private static readonly string DataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartEstimate");

        private const string ApiFileName = "api.dat";
        private const string KeyFileName = "key.dat";

        private static string ApiFilePath => Path.Combine(DataDirectory, ApiFileName);
        private static string EncryptionKeyPath => Path.Combine(DataDirectory, KeyFileName);

        /// <summary>
        /// Проверяет, существует ли именно файл api.dat в нашей папке.
        /// </summary>
        public static bool ApiKeyExists()
        {
            // Если папки нет — и файла нет
            if (!Directory.Exists(DataDirectory))
                return false;

            return File.Exists(ApiFilePath);
        }

        /// <summary>
        /// Сохраняет API-ключ, шифруя его SecretBox’ом.
        /// </summary>
        public static void SaveApiKey(string apiKey)
        {
            // Убедимся, что папка есть
            Directory.CreateDirectory(DataDirectory);

            // Генерируем или загружаем секретный ключ для шифрования
            byte[] key = GenerateOrLoadKey();

            // Создаём nonce и шифруем
            byte[] nonce = SodiumCore.GetRandomBytes(24);
            byte[] plain = Encoding.UTF8.GetBytes(apiKey);
            byte[] encrypted = SecretBox.Create(plain, nonce, key);

            // Склеиваем nonce + ciphertext
            byte[] combined = new byte[nonce.Length + encrypted.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(encrypted, 0, combined, nonce.Length, encrypted.Length);

            File.WriteAllBytes(ApiFilePath, combined);
        }

        /// <summary>
        /// Загружает и расшифровывает API-ключ, или возвращает null, если файла нет.
        /// </summary>
        public static string LoadApiKey()
        {
            if (!File.Exists(ApiFilePath))
                return null;

            byte[] key = GenerateOrLoadKey();
            byte[] combined = File.ReadAllBytes(ApiFilePath);

            // nonce: первые 24 байта
            byte[] nonce = new byte[24];
            // ciphertext: всё остальное
            byte[] ciphertext = new byte[combined.Length - nonce.Length];

            // копируем правильно
            Buffer.BlockCopy(combined, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(combined, nonce.Length, ciphertext, 0, ciphertext.Length);

            byte[] plain = SecretBox.Open(ciphertext, nonce, key);
            return Encoding.UTF8.GetString(plain);
        }


        /// <summary>
        /// Генерирует или читает из файла секретный ключ для шифрования.
        /// </summary>
        private static byte[] GenerateOrLoadKey()
        {
            // Убедимся, что папка есть
            Directory.CreateDirectory(DataDirectory);

            if (File.Exists(EncryptionKeyPath))
                return File.ReadAllBytes(EncryptionKeyPath);

            byte[] key = SodiumCore.GetRandomBytes(32);
            File.WriteAllBytes(EncryptionKeyPath, key);
            return key;
        }
    }
}
