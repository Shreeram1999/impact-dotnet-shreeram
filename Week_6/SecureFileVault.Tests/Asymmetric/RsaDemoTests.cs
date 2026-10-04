using System.Security.Cryptography;
using SecureFileVault.Asymmetric;

namespace SecureFileVault.Tests.Asymmetric;

public class RsaDemoTests
{
    [Fact]
    public void ShortMessage_RoundTripsThroughPublicEncryptPrivateDecrypt()
    {
        var (ciphertext, decrypted) = RsaDemo.RoundTrip("short secret");

        Assert.Equal("short secret", decrypted);
        Assert.Equal(256, ciphertext.Length); // always the key size, whatever the message length
    }

    [Fact]
    public void MaxPlaintext_For2048BitOaepSha256_Is190Bytes()
    {
        Assert.Equal(190, RsaDemo.MaxPlaintextBytes());
    }

    [Fact]
    public void DataAtTheLimit_Encrypts()
    {
        Assert.Equal(256, RsaDemo.EncryptRaw(new byte[190]).Length);
    }

    [Fact]
    public void DataOverTheLimit_CannotBeEncrypted_RsaIsNotForBulkData()
    {
        Assert.ThrowsAny<CryptographicException>(() => RsaDemo.EncryptRaw(new byte[191]));
    }
}
