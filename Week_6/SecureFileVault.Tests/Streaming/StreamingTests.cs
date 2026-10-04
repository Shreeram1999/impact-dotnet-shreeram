using System.Security.Cryptography;
using SecureFileVault.Integrity;
using SecureFileVault.Streaming;
using SecureFileVault.Tests.TestSupport;

namespace SecureFileVault.Tests.Streaming;

public class CbcFileStreamerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(250_000)]
    public void StreamRoundTrip_IsByteIdentical(int size)
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var data = RandomNumberGenerator.GetBytes(size);
        using var encrypted = new MemoryStream();
        using var decrypted = new MemoryStream();

        CbcFileStreamer.Encrypt(new MemoryStream(data), encrypted, key);
        encrypted.Position = 0;
        CbcFileStreamer.Decrypt(encrypted, decrypted, key);

        Assert.Equal(data, decrypted.ToArray());
    }

    [Fact]
    public void Ciphertext_IsIvPlusPaddedLength()
    {
        using var encrypted = new MemoryStream();

        CbcFileStreamer.Encrypt(new MemoryStream(new byte[20]), encrypted, RandomNumberGenerator.GetBytes(32));

        Assert.Equal(16 + 32, encrypted.Length); // 20 bytes pads up to two blocks
    }

    [Fact]
    public void FileRoundTrip_IsByteIdentical()
    {
        using var temp = new TempFolder();
        var key = RandomNumberGenerator.GetBytes(32);
        File.WriteAllBytes(temp.File("plain.bin"), RandomNumberGenerator.GetBytes(300_000));

        CbcFileStreamer.EncryptFile(temp.File("plain.bin"), temp.File("plain.cbc"), key);
        CbcFileStreamer.DecryptFile(temp.File("plain.cbc"), temp.File("plain.out"), key);

        Assert.True(DigestComparer.AreEqual(FileHasher.Sha256File(temp.File("plain.bin")), FileHasher.Sha256File(temp.File("plain.out"))));
    }
}

public class LargeFileGeneratorTests
{
    [Fact]
    public void Generate_WritesExactlyTheRequestedSize()
    {
        using var temp = new TempFolder();

        var length = LargeFileGenerator.Generate(temp.File("big.bin"), 2);

        Assert.Equal(2 * LargeFileGenerator.OneMegabyte, length);
        Assert.Equal(length, new FileInfo(temp.File("big.bin")).Length);
    }

    [Fact]
    public void Generate_ZeroMegabytes_IsRejected()
    {
        using var temp = new TempFolder();

        Assert.Throws<ArgumentOutOfRangeException>(() => LargeFileGenerator.Generate(temp.File("big.bin"), 0));
    }
}
