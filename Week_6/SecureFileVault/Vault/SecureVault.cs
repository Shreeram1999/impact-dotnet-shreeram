using System.Buffers.Binary;
using System.Security.Cryptography;
using SecureFileVault.KeyDerivation;

namespace SecureFileVault.Vault;

// The Week 6 deliverable: password -> PBKDF2 -> AES-GCM, streamed over a
// file of any size (100 MB+) without ever loading it whole into memory, and
// rejecting any tamper.
//
// AesGcm in .NET is one-shot (no CryptoStream mode), so the file is split
// into fixed-size chunks and each chunk is sealed with its own nonce and tag.
// The usual problems with "just encrypt chunks separately" are each closed
// off:
//   - Nonce reuse: nonce = 8 random bytes (per file) || 4-byte chunk index,
//     and the key itself is fresh per file (fresh salt), so no (key, nonce)
//     pair can ever repeat.
//   - Reordering / swapping chunks between files: the header and the chunk
//     index are fed in as associated data (AAD), so chunk 3 only verifies
//     as chunk 3 of THIS file.
//   - Truncation (cutting whole chunks off the end): the AAD also carries an
//     "is final chunk" flag. A file cut after a full chunk ends on a chunk
//     that was sealed as NOT final, which Decrypt reports.
//
// File format:
//   header: "SFV1" | iterations (int32 BE) | chunkSize (int32 BE) | salt (16) | noncePrefix (8)
//   chunk*: plaintextLength (int32 BE) | ciphertext (plaintextLength) | tag (16)
// The last chunk is the only one shorter than chunkSize (possibly 0 bytes),
// which is how Decrypt knows it's meant to be final.
public class SecureVault
{
    public const int DefaultChunkSize = 1024 * 1024;
    public const int MaxIterations = 10_000_000;
    private const int TagSize = 16;
    private const int NoncePrefixSize = 8;
    private static readonly byte[] Magic = "SFV1"u8.ToArray();
    private const int HeaderSize = 4 + 4 + 4 + PasswordKeyDeriver.SaltSizeBytes + NoncePrefixSize;

    private readonly int chunkSize;
    private readonly int iterations;

    public SecureVault(int chunkSize = DefaultChunkSize, int iterations = PasswordKeyDeriver.DefaultIterations)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
        this.chunkSize = chunkSize;
        this.iterations = iterations;
    }

    public void Encrypt(Stream plaintext, Stream output, string password)
    {
        var salt = PasswordKeyDeriver.GenerateSalt();
        var noncePrefix = RandomNumberGenerator.GetBytes(NoncePrefixSize);

        var header = new byte[HeaderSize];
        Magic.CopyTo(header, 0);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), iterations);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(8), chunkSize);
        salt.CopyTo(header, 12);
        noncePrefix.CopyTo(header, 12 + salt.Length);
        output.Write(header);

        var key = PasswordKeyDeriver.DeriveKey(password, salt, iterations);
        try
        {
            using var aes = new AesGcm(key, TagSize);
            var buffer = new byte[chunkSize];
            var ciphertext = new byte[chunkSize];
            var tag = new byte[TagSize];
            var lengthPrefix = new byte[4];

            for (uint index = 0; ; index++)
            {
                var read = ReadFull(plaintext, buffer);
                var isFinal = read < chunkSize;

                aes.Encrypt(Nonce(noncePrefix, index), buffer.AsSpan(0, read), ciphertext.AsSpan(0, read), tag,
                    AssociatedData(header, index, isFinal));

                BinaryPrimitives.WriteInt32BigEndian(lengthPrefix, read);
                output.Write(lengthPrefix);
                output.Write(ciphertext, 0, read);
                output.Write(tag);

                if (isFinal)
                    break;
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    // Throws AuthenticationTagMismatchException (a CryptographicException)
    // for a wrong password or any modified byte, and InvalidDataException
    // for a file that isn't a vault, is truncated, or has trailing data.
    // Chunks are verified one at a time, so plaintext from earlier chunks has
    // already been written when a later chunk fails - callers writing to a
    // file must delete the partial output on failure (VaultCli does).
    public void Decrypt(Stream input, Stream plaintextOutput, string password)
    {
        var header = new byte[HeaderSize];
        if (ReadFull(input, header) < HeaderSize || !header.AsSpan(0, 4).SequenceEqual(Magic))
            throw new InvalidDataException("Not a SecureFileVault file.");

        var fileIterations = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(4));
        var fileChunkSize = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(8));
        if (fileChunkSize <= 0 || fileChunkSize > 64 * 1024 * 1024)
            throw new InvalidDataException("Header has an invalid chunk size.");

        // The header isn't authenticated until the first chunk's tag is
        // checked, so bound the iteration count first - otherwise a tampered
        // header could ask for 2 billion PBKDF2 rounds and hang the tool.
        if (fileIterations < PasswordKeyDeriver.DefaultIterations || fileIterations > MaxIterations)
            throw new InvalidDataException("Header has an invalid iteration count.");

        var salt = header.AsSpan(12, PasswordKeyDeriver.SaltSizeBytes).ToArray();
        var noncePrefix = header.AsSpan(12 + salt.Length, NoncePrefixSize).ToArray();

        var key = PasswordKeyDeriver.DeriveKey(password, salt, fileIterations);
        try
        {
            using var aes = new AesGcm(key, TagSize);
            var ciphertext = new byte[fileChunkSize];
            var plaintext = new byte[fileChunkSize];
            var tag = new byte[TagSize];
            var lengthPrefix = new byte[4];

            for (uint index = 0; ; index++)
            {
                if (ReadFull(input, lengthPrefix) < 4)
                    throw new InvalidDataException("File is truncated: the final chunk is missing.");

                var length = BinaryPrimitives.ReadInt32BigEndian(lengthPrefix);
                if (length < 0 || length > fileChunkSize)
                    throw new InvalidDataException("Chunk length is out of range.");

                if (ReadFull(input, ciphertext.AsSpan(0, length)) < length || ReadFull(input, tag) < TagSize)
                    throw new InvalidDataException("File is truncated mid-chunk.");

                var isFinal = length < fileChunkSize;
                aes.Decrypt(Nonce(noncePrefix, index), ciphertext.AsSpan(0, length), tag, plaintext.AsSpan(0, length),
                    AssociatedData(header, index, isFinal));
                plaintextOutput.Write(plaintext, 0, length);

                if (isFinal)
                    break;
            }

            if (input.ReadByte() != -1)
                throw new InvalidDataException("Unexpected data after the final chunk.");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    public void EncryptFile(string inputPath, string outputPath, string password)
    {
        using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920);
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920);
        Encrypt(input, output, password);
    }

    public void DecryptFile(string inputPath, string outputPath, string password)
    {
        try
        {
            using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920);
            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920);
            Decrypt(input, output, password);
        }
        catch
        {
            // Never leave half-verified plaintext lying around.
            File.Delete(outputPath);
            throw;
        }
    }

    private static byte[] Nonce(byte[] prefix, uint index)
    {
        var nonce = new byte[NoncePrefixSize + 4];
        prefix.CopyTo(nonce, 0);
        BinaryPrimitives.WriteUInt32BigEndian(nonce.AsSpan(NoncePrefixSize), index);
        return nonce;
    }

    private static byte[] AssociatedData(byte[] header, uint index, bool isFinal)
    {
        var aad = new byte[header.Length + 5];
        header.CopyTo(aad, 0);
        BinaryPrimitives.WriteUInt32BigEndian(aad.AsSpan(header.Length), index);
        aad[^1] = isFinal ? (byte)1 : (byte)0;
        return aad;
    }

    // Stream.Read may return fewer bytes than asked even mid-stream; keep
    // reading until the buffer is full or the stream genuinely ends.
    private static int ReadFull(Stream stream, Span<byte> buffer)
    {
        var total = 0;
        int read;
        while (total < buffer.Length && (read = stream.Read(buffer[total..])) > 0)
            total += read;
        return total;
    }
}
