using System.Security.Cryptography;

namespace SecureFileVault.KeyDerivation;

// Task 6.7 - turning a human password into a 256-bit AES key with PBKDF2
// (Rfc2898DeriveBytes.Pbkdf2, HMAC-SHA256).
//
// Why a SALT: without one, the same password always produces the same key,
// so an attacker can precompute password->key tables once and reuse them
// against every user and every file. A random 16-byte salt per file makes
// every derivation unique, so all that precomputation is useless.
//
// Why ITERATIONS: a legitimate user derives a key once per login or file and
// never notices 100,000 rounds of HMAC. An attacker guessing passwords offline
// pays the same 100,000 rounds for EVERY guess. That multiplier is the
// whole point - it turns a billion-guesses-per-second attack into roughly
// ten thousand per second.
public static class PasswordKeyDeriver
{
    public const int SaltSizeBytes = 16;
    public const int KeySizeBytes = 32;
    public const int DefaultIterations = 100_000;

    public static byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(SaltSizeBytes);

    public static byte[] DeriveKey(string password, byte[] salt, int iterations = DefaultIterations)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        if (salt.Length < SaltSizeBytes)
            throw new ArgumentException($"Salt must be at least {SaltSizeBytes} bytes.", nameof(salt));
        if (iterations < DefaultIterations)
            throw new ArgumentOutOfRangeException(nameof(iterations), $"Use at least {DefaultIterations:N0} iterations.");

        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, KeySizeBytes);
    }
}
