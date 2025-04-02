using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using System.Text;

namespace AwsVpnClient.Infrastructure.Security
{
    public static class CredentialStorage
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AwsVpnClient", "aws_credentials.dat");

        public static void Save(string accessKey, string secretKey, string region, string? publicIp = null, string? serverKey = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var raw = $"{accessKey}||{secretKey}||{region}||{publicIp}||{serverKey}";
            var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(raw), null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(FilePath, encrypted);
        }

        public static (string accessKey, string secretKey, string region, string? publicIp, string? serverKey)? Load()
        {
            if (!File.Exists(FilePath)) return null;

            var encrypted = File.ReadAllBytes(FilePath);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var raw = Encoding.UTF8.GetString(decrypted);
            var parts = raw.Split(new[] { "||" }, StringSplitOptions.None);

            if (parts.Length < 3) return null;
            var ip = parts.Length >= 4 ? parts[3] : null;
            var key = parts.Length >= 5 ? parts[4] : null;
            return (parts[0], parts[1], parts[2], ip, key);
        }

        public static void Clear()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

    }
}
