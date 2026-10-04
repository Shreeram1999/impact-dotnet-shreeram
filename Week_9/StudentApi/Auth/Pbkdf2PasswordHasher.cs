using System.Security.Cryptography;

namespace StudentApi.Auth;

// Task 6.15 - "reuse PBKDF2 to store the user's password hash". Same
// primitive as SecureFileVault's PasswordKeyDeriver (Task 6.7), used for a
// different job: there the derived bytes are an AES key, here they're a
// verifier. Stored format, all in one string so nothing else is needed to
// verify later:
//   PBKDF2-SHA256$<iterations>$<base64 salt>$<base64 hash>
// Keeping the iteration count in the string means it can be raised later
// without breaking existing users - old hashes still verify at their old
// count.
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Scheme = "PBKDF2-SHA256";
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Scheme}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    // Never throws for a malformed stored value - a corrupt row just means
    // "this password doesn't verify", not a 500 on the login endpoint.
    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != Scheme || !int.TryParse(parts[1], out var iterations) || iterations <= 0)
            return false;

        byte[] salt, expected;
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

        // Task 6.11 again - constant-time, so response timing can't reveal
        // how many leading bytes of a guess were right.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
