using System.Security.Cryptography;
using System.Text;

namespace StdbModule.Utils;

public static class HashUtils
{
    public static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return ComputeHash(bytes);
    }
    
    public static string ComputeHash(byte[] data)
    {
        var hash = Sha256.ComputeHash(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
    
    public static bool VerifyHash(byte[] data, string expectedHashHex)
    {
        var computed = ComputeHash(data);
        return string.Equals(computed, expectedHashHex, StringComparison.OrdinalIgnoreCase);
    }
    public static bool VerifyHash(string input, string expectedHashHex)
    {
        var computed = ComputeHash(input);
        return string.Equals(computed, expectedHashHex, StringComparison.OrdinalIgnoreCase);
    }
}