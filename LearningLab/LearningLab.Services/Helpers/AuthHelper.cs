using System.Security.Cryptography;

namespace LearningLab.Services.Helpers;

public static class AuthHelper
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static (string PasswordHash, string PasswordSalt) HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(password));
        }

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = GenerateHash(password, saltBytes);

        return (
            Convert.ToBase64String(hashBytes),
            Convert.ToBase64String(saltBytes));
    }

    public static bool VerifyPassword(string password, string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(passwordHash) ||
            string.IsNullOrWhiteSpace(passwordSalt))
        {
            return false;
        }

        try
        {
            var saltBytes = Convert.FromBase64String(passwordSalt);
            var storedHashBytes = Convert.FromBase64String(passwordHash);
            var loginHashBytes = GenerateHash(password, saltBytes);

            return CryptographicOperations.FixedTimeEquals(storedHashBytes, loginHashBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] GenerateHash(string password, byte[] saltBytes)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);
    }
}
