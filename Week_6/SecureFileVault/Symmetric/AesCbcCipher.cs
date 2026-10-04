using System.Security.Cryptography;
using System.Text;

namespace SecureFileVault.Symmetric;

// Task 6.4/6.5/6.6 - AES-256 in CBC mode, through a CryptoStream over a
// MemoryStream.
//
// Task 6.5 - the IV rule: a fresh random IV for EVERY encryption, even with
// the same key and the same plaintext. CBC XORs the first block with the IV,
// so a reused IV makes identical plaintexts produce identical ciphertexts -
// an observer learns "these two messages are the same" without the key. The
// IV isn't secret, so it's simply stored in front of the ciphertext
// (Encrypt returns iv || ciphertext) and read back out by Decrypt.
//
// Task 6.6 - the integrity gap: CBC gives confidentiality ONLY. Decrypting
// with the wrong key (or a tampered ciphertext) is only ever "noticed" if
// the garbage happens to end in invalid PKCS7 padding, which surfaces as a
// CryptographicException. Roughly 1 time in 256 the garbage ends in valid
// padding anyway, and Decrypt quietly returns nonsense. There is no
// tamper signal at all - and an attacker who can tell "bad padding" apart
// from other errors can use that to decrypt data (a padding oracle). That's
// why the vault itself uses AES-GCM instead (Authenticated/AesGcmCipher.cs).
public static class AesCbcCipher
{
    public const int KeySizeBytes = 32;
    public const int IvSizeBytes = 16;

    public static byte[] GenerateKey() => RandomNumberGenerator.GetBytes(KeySizeBytes);

    public static byte[] Encrypt(string plaintext, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var output = new MemoryStream();
        output.Write(aes.IV);
        using (var crypto = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        {
            var bytes = Encoding.UTF8.GetBytes(plaintext);
            crypto.Write(bytes);
        } // disposing the CryptoStream flushes the final (padded) block

        return output.ToArray();
    }

    public static string Decrypt(byte[] ivAndCiphertext, byte[] key)
    {
        if (ivAndCiphertext.Length < IvSizeBytes)
            throw new ArgumentException("Payload is shorter than an IV.", nameof(ivAndCiphertext));

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = ivAndCiphertext[..IvSizeBytes];

        using var input = new MemoryStream(ivAndCiphertext, IvSizeBytes, ivAndCiphertext.Length - IvSizeBytes);
        using var crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var reader = new StreamReader(crypto, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static byte[] ExtractIv(byte[] ivAndCiphertext) => ivAndCiphertext[..IvSizeBytes];
}
