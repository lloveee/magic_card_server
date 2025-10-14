using System.Security.Cryptography;
using System.Text;
using SpacetimeDB;

namespace StdbModule.Utils;

public static class RSAEncryptionUtil
{
    public static (string publicKey, string privateKey) GenerateKeyPair(int keySize = 2048)
    {
        using (var rsa = RSA.Create(keySize))
        {
            var publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            var privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
            return (publicKey, privateKey);
        }
    }

    public static string DecryptWithPrivateKey(string cipherText, string privateKey)
    {
        using (var rsa = RSA.Create())
        {
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
            var bytes = Convert.FromBase64String(cipherText);
            var decrypted = rsa.Decrypt(bytes, RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
    
    public static string SignData(string plainText, string privateKey)
    {
        using (var rsa = RSA.Create())
        {
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
            var data = Encoding.UTF8.GetBytes(plainText);
            var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }
    }

}