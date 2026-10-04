using System.Buffers.Binary;
using System.Security.Cryptography;
using SecureFileVault.Tests.TestSupport;
using SecureFileVault.Vault;

namespace SecureFileVault.Tests.Vault;

// The vault's core logic, with a tiny 1 KB chunk size so multi-chunk
// behaviour (boundaries, reordering, truncation) is exercised on small
// in-memory buffers instead of 100 MB files.
//
// Layout with ChunkSize = 1024:
//   header 36 bytes | chunk = length(4) + ciphertext(<=1024) + tag(16)
public class SecureVaultTests
{
    private const int ChunkSize = 1024;
    private const int HeaderSize = 36;
    private const int FullChunkSize = 4 + ChunkSize + 16;
    private const string Password = "Week6-Vault!";

    private static readonly SecureVault Vault = new(ChunkSize);

    private static byte[] Encrypt(byte[] data, string password = Password)
    {
        using var output = new MemoryStream();
        Vault.Encrypt(new MemoryStream(data), output, password);
        return output.ToArray();
    }

    private static byte[] Decrypt(byte[] vaultBytes, string password = Password)
    {
        using var output = new MemoryStream();
        Vault.Decrypt(new MemoryStream(vaultBytes), output, password);
        return output.ToArray();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(ChunkSize - 1)]
    [InlineData(ChunkSize)]
    [InlineData(ChunkSize + 1)]
    [InlineData(ChunkSize * 5 + 17)]
    public void RoundTrip_IsByteIdentical_AcrossChunkBoundaries(int size)
    {
        var data = RandomNumberGenerator.GetBytes(size);

        Assert.Equal(data, Decrypt(Encrypt(data)));
    }

    [Fact]
    public void ExactMultipleOfChunkSize_EndsWithAnEmptyFinalChunk()
    {
        var vaultBytes = Encrypt(new byte[ChunkSize * 2]);

        Assert.Equal(HeaderSize + 2 * FullChunkSize + 4 + 16, vaultBytes.Length);
    }

    [Fact]
    public void SameFileTwice_ProducesDifferentCiphertext()
    {
        var data = new byte[100];

        Assert.NotEqual(Encrypt(data), Encrypt(data));
    }

    [Fact]
    public void WrongPassword_IsRejected()
    {
        var vaultBytes = Encrypt(new byte[100]);

        Assert.Throws<AuthenticationTagMismatchException>(() => Decrypt(vaultBytes, "wrong-password"));
    }

    [Theory]
    [InlineData(HeaderSize + 4)]                       // first ciphertext byte
    [InlineData(HeaderSize + 4 + ChunkSize)]           // first tag byte of chunk 0
    [InlineData(HeaderSize + FullChunkSize + 4 + 10)]  // inside chunk 1
    [InlineData(12)]                                   // salt (header)
    [InlineData(HeaderSize - 1)]                       // nonce prefix (header)
    public void SingleBitFlip_AnywhereAuthenticated_IsRejected(int offset)
    {
        var vaultBytes = Encrypt(RandomNumberGenerator.GetBytes(ChunkSize * 2 + 50));
        vaultBytes[offset] ^= 0x01;

        Assert.Throws<AuthenticationTagMismatchException>(() => Decrypt(vaultBytes));
    }

    [Fact]
    public void SwappingTwoChunks_IsRejected()
    {
        var vaultBytes = Encrypt(RandomNumberGenerator.GetBytes(ChunkSize * 2 + 50));
        var chunk0 = vaultBytes.AsSpan(HeaderSize, FullChunkSize).ToArray();
        var chunk1 = vaultBytes.AsSpan(HeaderSize + FullChunkSize, FullChunkSize).ToArray();
        chunk1.CopyTo(vaultBytes, HeaderSize);
        chunk0.CopyTo(vaultBytes, HeaderSize + FullChunkSize);

        Assert.Throws<AuthenticationTagMismatchException>(() => Decrypt(vaultBytes));
    }

    [Fact]
    public void DroppingTheFinalChunk_IsDetectedAsTruncation()
    {
        var vaultBytes = Encrypt(new byte[ChunkSize * 2]);
        var withoutFinal = vaultBytes[..(HeaderSize + 2 * FullChunkSize)];

        var ex = Assert.Throws<InvalidDataException>(() => Decrypt(withoutFinal));
        Assert.Contains("truncated", ex.Message);
    }

    [Fact]
    public void CuttingBytesOffMidChunk_IsDetected()
    {
        var vaultBytes = Encrypt(new byte[1500]);

        Assert.Throws<InvalidDataException>(() => Decrypt(vaultBytes[..^10]));
    }

    [Fact]
    public void TrailingDataAfterTheFinalChunk_IsRejected()
    {
        var vaultBytes = Encrypt(new byte[100]);

        Assert.Throws<InvalidDataException>(() => Decrypt([.. vaultBytes, 0x00]));
    }

    [Fact]
    public void ChunkLengthLargerThanChunkSize_IsRejected()
    {
        var vaultBytes = Encrypt(new byte[100]);
        BinaryPrimitives.WriteInt32BigEndian(vaultBytes.AsSpan(HeaderSize), ChunkSize + 1);

        Assert.Throws<InvalidDataException>(() => Decrypt(vaultBytes));
    }

    [Fact]
    public void WrongMagic_IsNotAVaultFile()
    {
        var vaultBytes = Encrypt(new byte[100]);
        vaultBytes[0] = (byte)'X';

        Assert.Throws<InvalidDataException>(() => Decrypt(vaultBytes));
    }

    [Fact]
    public void FileShorterThanTheHeader_IsNotAVaultFile()
    {
        Assert.Throws<InvalidDataException>(() => Decrypt(new byte[10]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void InvalidChunkSizeInHeader_IsRejected(int chunkSize)
    {
        var vaultBytes = Encrypt(new byte[100]);
        BinaryPrimitives.WriteInt32BigEndian(vaultBytes.AsSpan(8), chunkSize);

        Assert.Throws<InvalidDataException>(() => Decrypt(vaultBytes));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void OutOfRangeIterationCountInHeader_IsRejectedBeforeDerivingAKey(int iterations)
    {
        var vaultBytes = Encrypt(new byte[100]);
        BinaryPrimitives.WriteInt32BigEndian(vaultBytes.AsSpan(4), iterations);

        Assert.Throws<InvalidDataException>(() => Decrypt(vaultBytes));
    }

    [Fact]
    public void Constructor_RejectsANonPositiveChunkSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SecureVault(0));
    }

    [Fact]
    public void FileRoundTrip_IsByteIdentical()
    {
        using var temp = new TempFolder();
        var data = RandomNumberGenerator.GetBytes(5000);
        File.WriteAllBytes(temp.File("in.bin"), data);

        Vault.EncryptFile(temp.File("in.bin"), temp.File("in.sfv"), Password);
        Vault.DecryptFile(temp.File("in.sfv"), temp.File("out.bin"), Password);

        Assert.Equal(data, File.ReadAllBytes(temp.File("out.bin")));
    }

    [Fact]
    public void DecryptFile_OnTamper_DeletesThePartialOutput()
    {
        using var temp = new TempFolder();
        File.WriteAllBytes(temp.File("in.bin"), RandomNumberGenerator.GetBytes(ChunkSize * 3));
        Vault.EncryptFile(temp.File("in.bin"), temp.File("in.sfv"), Password);

        // Tamper with the LAST chunk, so earlier chunks have already been
        // written to out.bin by the time the failure is detected.
        var vaultBytes = File.ReadAllBytes(temp.File("in.sfv"));
        vaultBytes[HeaderSize + 2 * FullChunkSize + 4 + 1] ^= 0x01;
        File.WriteAllBytes(temp.File("in.sfv"), vaultBytes);

        Assert.Throws<AuthenticationTagMismatchException>(() =>
            Vault.DecryptFile(temp.File("in.sfv"), temp.File("out.bin"), Password));
        Assert.False(File.Exists(temp.File("out.bin")));
    }
}
