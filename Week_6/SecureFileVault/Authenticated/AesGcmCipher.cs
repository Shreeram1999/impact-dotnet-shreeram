using System.Security.Cryptography;
using SecureFileVault.KeyDerivation;

namespace SecureFileVault.Authenticated;

// Task 6.8/6.9 - AES-GCM: confidentiality AND integrity in one primitive.
//
// Layout of the single blob Encrypt returns (everything Decrypt needs except
// the password, none of it secret):
//   salt (16) || nonce (12) || tag (16) || ciphertext (same length as plaintext)
//
// Task 6.9 - contrast with Task 6.6: GCM computes a 16-byte authentication
// tag over the ciphertext, and Decrypt recomputes and checks it BEFORE
// handing anything back. Flip a single bit anywhere in the nonce, tag or
// ciphertext (or use the wrong password) and Decrypt throws
// AuthenticationTagMismatchException, every time, instead of returning
// garbage. CBC could only stumble over broken padding by luck. This is what
// "authenticated encryption" buys you.
//
// The nonce rule is stricter than CBC's IV rule: reusing a nonce under one
// key doesn't just leak equality - it leaks the XOR of the two plaintexts
// AND lets an attacker forge tags. Here each call derives a fresh key from a
// fresh salt AND draws a fresh random nonce, so a repeat is impossible.
public static class AesGcmCipher
{
    public const int NonceSizeBytes = 12;
    public const int TagSizeBytes = 16;
    public const int HeaderSizeBytes = PasswordKeyDeriver.SaltSizeBytes + NonceSizeBytes + TagSizeBytes;

    public static byte[] Encrypt(byte[] plaintext, string password)
    {
        var salt = PasswordKeyDeriver.GenerateSalt();
        var key = PasswordKeyDeriver.DeriveKey(password, salt);
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var tag = new byte[TagSizeBytes];
        var ciphertext = new byte[plaintext.Length];

        using (var aes = new AesGcm(key, TagSizeBytes))
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

        CryptographicOperations.ZeroMemory(key);
        return [.. salt, .. nonce, .. tag, .. ciphertext];
    }

    public static byte[] Decrypt(byte[] blob, string password)
    {
        if (blob.Length < HeaderSizeBytes)
            throw new ArgumentException("Blob is too short to contain salt, nonce and tag.", nameof(blob));

        var span = blob.AsSpan();
        var salt = span[..PasswordKeyDeriver.SaltSizeBytes].ToArray();
        var nonce = span.Slice(PasswordKeyDeriver.SaltSizeBytes, NonceSizeBytes);
        var tag = span.Slice(PasswordKeyDeriver.SaltSizeBytes + NonceSizeBytes, TagSizeBytes);
        var ciphertext = span[HeaderSizeBytes..];
        var plaintext = new byte[ciphertext.Length];

        var key = PasswordKeyDeriver.DeriveKey(password, salt);
        try
        {
            using var aes = new AesGcm(key, TagSizeBytes);
            aes.Decrypt(nonce, ciphertext, tag, plaintext); // throws on any tamper
            return plaintext;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }
}
