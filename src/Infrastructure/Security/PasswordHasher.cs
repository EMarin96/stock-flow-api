using System.Security.Cryptography;
using StockFlow.Application.Common.Security;

namespace StockFlow.Infrastructure.Security;

/// <summary>
/// PBKDF2-based password hasher (see plan.md — Decisions: zero new NuGet
/// packages for this part, sidesteps the "no new dependency without
/// checking" hard limit entirely for hashing). Stored as
/// "{iterations}.{base64 salt}.{base64 hash}" so the iteration count and
/// salt travel with the hash.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSizeInBytes = 16;
    private const int HashSizeInBytes = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeInBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSizeInBytes);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        // Constant-time comparison — avoids a timing side-channel on password
        // checks (see plan.md — Implementation).
        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }
}
