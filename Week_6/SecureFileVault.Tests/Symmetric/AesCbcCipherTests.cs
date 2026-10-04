using System.Security.Cryptography;
using SecureFileVault.Symmetric;

namespace SecureFileVault.Tests.Symmetric;

public class AesCbcCipherTests
{
    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("exactly sixteen!")]
    [InlineData("a longer message that spans several AES blocks of sixteen bytes each")]
    public void EncryptThenDecrypt_RoundTrips(string plaintext)
    {
        var key = AesCbcCipher.GenerateKey();

        Assert.Equal(plaintext, AesCbcCipher.Decrypt(AesCbcCipher.Encrypt(plaintext, key), key));
    }

    [Fact]
    public void GenerateKey_Is256Bits()
    {
        Assert.Equal(32, AesCbcCipher.GenerateKey().Length);
    }

    [Fact]
    public void SamePlaintextSameKey_ProducesDifferentCiphertext_BecauseTheIvIsFresh()
    {
        var key = AesCbcCipher.GenerateKey();

        var first = AesCbcCipher.Encrypt("same plaintext", key);
        var second = AesCbcCipher.Encrypt("same plaintext", key);

        Assert.NotEqual(AesCbcCipher.ExtractIv(first), AesCbcCipher.ExtractIv(second));
        Assert.NotEqual(first, second);
        Assert.Equal("same plaintext", AesCbcCipher.Decrypt(first, key));
        Assert.Equal("same plaintext", AesCbcCipher.Decrypt(second, key));
    }

    // Task 6.6 - the wrong key is USUALLY caught as bad padding, but ~1 in
    // 256 runs the garbage happens to end in valid padding and comes back
    // with no error at all. Either way the real plaintext is never returned -
    // and the fact this test has to allow both outcomes IS the integrity gap.
    [Fact]
    public void WrongKey_EitherThrowsOrReturnsGarbage_NeverThePlaintext()
    {
        var payload = AesCbcCipher.Encrypt("top secret marks", AesCbcCipher.GenerateKey());

        try
        {
            var result = AesCbcCipher.Decrypt(payload, AesCbcCipher.GenerateKey());
            Assert.NotEqual("top secret marks", result);
        }
        catch (CryptographicException)
        {
            // the common case: broken padding
        }
    }

    // The common case, measured: across 20 wrong-key decrypts, nearly every
    // one surfaces as CryptographicException (bad padding). Allowing a few
    // misses keeps the test stable (a miss is ~1/256 per attempt).
    [Fact]
    public void WrongKey_AlmostAlwaysThrowsCryptographicException()
    {
        var key = AesCbcCipher.GenerateKey();
        var wrongKey = AesCbcCipher.GenerateKey();
        var thrown = 0;
        for (var i = 0; i < 20; i++)
        {
            try { AesCbcCipher.Decrypt(AesCbcCipher.Encrypt("padding check", key), wrongKey); }
            catch (CryptographicException) { thrown++; }
        }

        Assert.True(thrown >= 15, $"Expected bad padding on almost every wrong-key decrypt, saw {thrown}/20.");
    }

    [Fact]
    public void Decrypt_PayloadShorterThanIv_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => AesCbcCipher.Decrypt(new byte[5], AesCbcCipher.GenerateKey()));
    }
}
