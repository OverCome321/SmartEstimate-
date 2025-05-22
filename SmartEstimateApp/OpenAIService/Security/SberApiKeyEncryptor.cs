using Sodium;
using System.Text;

namespace OpenAIService.Security;

public static class SberApiKeyEncryptor
{
    private static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SmartEstimate");
    private const string ApiFileName = "sber_api.dat";
    private const string KeyFileName = "sber_key.dat";

    private static string ApiFilePath => Path.Combine(DataDirectory, ApiFileName);
    private static string EncryptionKeyPath => Path.Combine(DataDirectory, KeyFileName);

    public static bool ApiKeyExists() => File.Exists(ApiFilePath);

    public static void SaveApiKey(string apiKey)
    {
        Directory.CreateDirectory(DataDirectory);
        byte[] key = GenerateOrLoadKey();
        byte[] nonce = SodiumCore.GetRandomBytes(24);
        byte[] plain = Encoding.UTF8.GetBytes(apiKey);
        byte[] encrypted = SecretBox.Create(plain, nonce, key);
        byte[] combined = nonce.Concat(encrypted).ToArray();
        File.WriteAllBytes(ApiFilePath, combined);
    }

    public static string? LoadApiKey()
    {
        if (!File.Exists(ApiFilePath)) return null;
        byte[] key = GenerateOrLoadKey();
        byte[] combined = File.ReadAllBytes(ApiFilePath);
        byte[] nonce = combined.Take(24).ToArray();
        byte[] cipher = combined.Skip(24).ToArray();
        byte[] plain = SecretBox.Open(cipher, nonce, key);
        return Encoding.UTF8.GetString(plain);
    }

    private static byte[] GenerateOrLoadKey()
    {
        Directory.CreateDirectory(DataDirectory);
        if (File.Exists(EncryptionKeyPath))
            return File.ReadAllBytes(EncryptionKeyPath);
        byte[] key = SodiumCore.GetRandomBytes(32);
        File.WriteAllBytes(EncryptionKeyPath, key);
        return key;
    }
}
