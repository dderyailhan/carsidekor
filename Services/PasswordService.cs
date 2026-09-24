using System.Security.Cryptography;

namespace CarsiDekor.Web.Services;

/// <summary>
/// Şifreleri PBKDF2 (SHA-256) ile hash'ler. Şifre veritabanında asla açık metin tutulmaz.
/// Saklanan format: v1.{tekrar}.{tuz}.{hash}
/// </summary>
public static class PasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 600_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 4 || parts[0] != "v1" || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        // Süre farkından bilgi sızmasın diye sabit sürede karşılaştırılır
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
