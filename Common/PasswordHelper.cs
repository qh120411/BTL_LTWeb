using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace BTL_LTWEB.Common;
public static class PasswordHelper
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Subject = new();
    public static string HashPassword(string password) => Hasher.HashPassword(Subject, password);
    public static bool VerifyPassword(string password, string? hash) => Verify(password, hash, out _);
    public static bool Verify(string password, string? hash, out bool upgrade)
    {
        upgrade = false;
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;
        // Chỉ tương thích hash SHA-256 thật; không chấp nhận chuỗi thường / HASH_msv.
        if (hash.Length == 64 && hash.All(Uri.IsHexDigit))
        {
            var ok = CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(password)), Convert.FromHexString(hash));
            upgrade = ok;
            return ok;
        }

        try
        {
            var result = Hasher.VerifyHashedPassword(Subject, hash, password);
            upgrade = result == PasswordVerificationResult.SuccessRehashNeeded;
            return result != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
