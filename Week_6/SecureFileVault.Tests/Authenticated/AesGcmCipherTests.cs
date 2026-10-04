using System.Security.Cryptography;
using System.Text;
using SecureFileVault.Authenticated;

namespace SecureFileVault.Tests.Authenticated;

public class AesGcmCipherTests
{
    private const string Password = "vault-password";

    [Theory]
    [InlineData("")]
    [InlineData("exam results")]
    public void EncryptThenDecrypt_RoundTripsAndTheTagVerifies(string text)
    {
        var blob = AesGcmCipher.Encrypt(Encoding.UTF8.GetBytes(text), Password);

        Assert.Equal(text, Encoding.UTF8.GetString(AesGcmCipher.Decrypt(blob, Password)));
    }

    [Fact]
    public void Blob_IsSaltNonceTagThenCiphertext()
    {
        var blob = AesGcmCipher.Encrypt(new byte[10], Password);

        Assert.Equal(16 + 12 + 16 + 10, blob.Length);
    }

    [Fact]
    public void SamePlaintextTwice_ProducesDifferentBlobs()
    {
        var plaintext = Encoding.UTF8.GetBytes("same");

        Assert.NotEqual(AesGcmCipher.Encrypt(plaintext, Password), AesGcmCipher.Encrypt(plaintext, Password));
    }

    // Task 6.9 - a single flipped bit anywhere authenticated (nonce, tag or
    // ciphertext) must throw, never return garbage.
    [Theory]
    [InlineData(16)]          // first nonce byte
    [InlineData(16 + 12)]     // first tag byte
    [InlineData(16 + 12 + 16)] // first ciphertext byte
    public void SingleByteTamper_IsRejected(int offset)
    {
        var blob = AesGcmCipher.Encrypt(Encoding.UTF8.GetBytes("exam results"), Password);
        blob[offset] ^= 0x01;

        Assert.Throws<AuthenticationTagMismatchException>(() => AesGcmCipher.Decrypt(blob, Password));
    }

    [Fact]
    public void TamperedSalt_DerivesTheWrongKey_AndIsRejected()
    {
        var blob = AesGcmCipher.Encrypt(Encoding.UTF8.GetBytes("exam results"), Password);
        blob[0] ^= 0x01;

        Assert.Throws<AuthenticationTagMismatchException>(() => AesGcmCipher.Decrypt(blob, Password));
    }

    [Fact]
    public void WrongPassword_IsRejected()
    {
        var blob = AesGcmCipher.Encrypt(Encoding.UTF8.GetBytes("exam results"), Password);

        Assert.Throws<AuthenticationTagMismatchException>(() => AesGcmCipher.Decrypt(blob, "not-the-password"));
    }

    [Fact]
    public void BlobShorterThanHeader_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => AesGcmCipher.Decrypt(new byte[AesGcmCipher.HeaderSizeBytes - 1], Password));
    }
}
