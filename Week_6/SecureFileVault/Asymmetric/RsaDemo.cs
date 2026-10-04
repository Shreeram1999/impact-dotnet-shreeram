using System.Security.Cryptography;
using System.Text;

namespace SecureFileVault.Asymmetric;

// Task 6.13 (understand - concept + tiny demo; the project itself does not
// use RSA).
//
// Which key does what:
//   ENCRYPT with the PUBLIC key  -> only the private-key holder can decrypt
//                                   (confidentiality: anyone can send to me).
//   SIGN    with the PRIVATE key -> anyone with the public key can verify
//                                   (authenticity: only I could have sent it).
//
// The size limit: RSA-2048 with OAEP-SHA256 can encrypt at most
// 256 - 2*32 - 2 = 190 bytes in one operation, and it's thousands of times
// slower than AES. So RSA never encrypts bulk data. Hybrid RSA+AES (what TLS
// does conceptually): generate a random AES key, encrypt the DATA with AES,
// and encrypt only the 32-byte AES KEY with RSA. You get the fast bulk
// cipher, plus a way to share its key without ever having met. (Modern TLS
// 1.3 actually uses ephemeral Diffie-Hellman for the key exchange and RSA
// only for signing the handshake - same split of roles, different
// primitive.)
public static class RsaDemo
{
    public const int KeySizeBits = 2048;

    // OAEP padding overhead = 2 * hashLength + 2 bytes.
    public static int MaxPlaintextBytes(int keySizeBits = KeySizeBits) => keySizeBits / 8 - 2 * SHA256.HashSizeInBytes - 2;

    public static (byte[] Ciphertext, string Decrypted) RoundTrip(string message)
    {
        using var rsa = RSA.Create(KeySizeBits);

        // Simulate the real flow: the sender only ever has the PUBLIC key.
        using var publicOnly = RSA.Create();
        publicOnly.ImportRSAPublicKey(rsa.ExportRSAPublicKey(), out _);

        var ciphertext = publicOnly.Encrypt(Encoding.UTF8.GetBytes(message), RSAEncryptionPadding.OaepSHA256);
        var decrypted = Encoding.UTF8.GetString(rsa.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256));
        return (ciphertext, decrypted);
    }

    // Shows the bulk-data limit: anything over MaxPlaintextBytes() throws.
    public static byte[] EncryptRaw(byte[] data)
    {
        using var rsa = RSA.Create(KeySizeBits);
        return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
    }
}
