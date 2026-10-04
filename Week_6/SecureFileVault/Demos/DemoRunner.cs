using System.Security.Cryptography;
using System.Text;
using SecureFileVault.Asymmetric;
using SecureFileVault.Authenticated;
using SecureFileVault.FileHandling;
using SecureFileVault.Fundamentals;
using SecureFileVault.Integrity;
using SecureFileVault.KeyDerivation;
using SecureFileVault.Symmetric;

namespace SecureFileVault.Demos;

// `SecureFileVault demo` - runs every Day 1-5 task once and prints the
// evidence each task's "done when" asks for. Works in a throwaway temp
// folder that's deleted afterwards.
public class DemoRunner
{
    private readonly TextWriter output;

    public DemoRunner(TextWriter output)
    {
        this.output = output;
    }

    public void RunAll()
    {
        var workDir = Directory.CreateTempSubdirectory("sfv-demo-").FullName;
        try
        {
            FileIo(workDir);
            EncodingHashing();
            AesCbc();
            Pbkdf2();
            Gcm();
            HashingAndHmac(workDir);
            ConstantTime();
            Rsa();
        }
        finally
        {
            Directory.Delete(workDir, recursive: true);
        }
    }

    private void FileIo(string workDir)
    {
        Header("6.1 FileStream I/O");
        var notes = Path.Combine(workDir, "notes.txt");
        FileStreamBasics.WriteText(notes, "line 1\n");
        FileStreamBasics.AppendText(notes, "line 2\n");
        output.WriteLine($"write + append -> {FileStreamBasics.ReadText(notes).Replace("\n", "\\n")}");

        var source = Path.Combine(workDir, "source.bin");
        File.WriteAllBytes(source, RandomNumberGenerator.GetBytes(1_000_000));
        var copy = Path.Combine(workDir, "copy.bin");
        var chunks = FileStreamBasics.CopyInChunks(source, copy);
        var identical = DigestComparer.AreEqual(FileHasher.Sha256File(source), FileHasher.Sha256File(copy));
        output.WriteLine($"chunked copy of 1,000,000 bytes in {chunks} x 4 KB reads -> byte-identical: {identical}");
    }

    private void EncodingHashing()
    {
        Header("6.2 Encoding vs hashing vs encryption");
        var b64 = EncodingVsHashing.ToBase64("Student Portal");
        output.WriteLine($"Base64: {b64} -> back: {EncodingVsHashing.FromBase64(b64)}");
        output.WriteLine($"SHA-256 (\"a\")      : {EncodingVsHashing.Sha256Hex("a")}");
        output.WriteLine($"SHA-256 (1000 x \"a\"): {EncodingVsHashing.Sha256Hex(new string('a', 1000))}  (same 64 hex chars, no way back)");
    }

    private void AesCbc()
    {
        Header("6.4-6.6 AES-256-CBC");
        var key = AesCbcCipher.GenerateKey();
        var first = AesCbcCipher.Encrypt("same plaintext", key);
        var second = AesCbcCipher.Encrypt("same plaintext", key);
        output.WriteLine($"IV #1 {Convert.ToHexString(AesCbcCipher.ExtractIv(first))}");
        output.WriteLine($"IV #2 {Convert.ToHexString(AesCbcCipher.ExtractIv(second))}");
        output.WriteLine($"same plaintext, same key -> ciphertexts differ: {!first.AsSpan().SequenceEqual(second)}");
        output.WriteLine($"both decrypt: '{AesCbcCipher.Decrypt(first, key)}' / '{AesCbcCipher.Decrypt(second, key)}'");
        try
        {
            AesCbcCipher.Decrypt(first, AesCbcCipher.GenerateKey());
            output.WriteLine("wrong key: returned garbage with no error (the ~1-in-256 valid-padding case) - no integrity!");
        }
        catch (CryptographicException ex)
        {
            output.WriteLine($"wrong key -> CryptographicException ({ex.Message.Trim()}) - only broken padding noticed it");
        }
    }

    private void Pbkdf2()
    {
        Header("6.7 PBKDF2 (100,000 iterations, SHA-256, 32-byte key)");
        var salt = PasswordKeyDeriver.GenerateSalt();
        var a = PasswordKeyDeriver.DeriveKey("correct horse", salt);
        var b = PasswordKeyDeriver.DeriveKey("correct horse", salt);
        var c = PasswordKeyDeriver.DeriveKey("correct horse", PasswordKeyDeriver.GenerateSalt());
        output.WriteLine($"same password + same salt -> same key: {a.AsSpan().SequenceEqual(b)}");
        output.WriteLine($"same password + new salt  -> same key: {a.AsSpan().SequenceEqual(c)}");
    }

    private void Gcm()
    {
        Header("6.8-6.9 AES-GCM (salt || nonce || tag || ciphertext)");
        var blob = AesGcmCipher.Encrypt(Encoding.UTF8.GetBytes("exam results"), "vault-password");
        output.WriteLine($"blob = {blob.Length} bytes ({AesGcmCipher.HeaderSizeBytes} header + {blob.Length - AesGcmCipher.HeaderSizeBytes} ciphertext)");
        output.WriteLine($"round-trip: '{Encoding.UTF8.GetString(AesGcmCipher.Decrypt(blob, "vault-password"))}' (tag verified)");

        blob[^1] ^= 0x01;
        try
        {
            AesGcmCipher.Decrypt(blob, "vault-password");
            output.WriteLine("tampered blob decrypted - THIS SHOULD NEVER HAPPEN");
        }
        catch (AuthenticationTagMismatchException)
        {
            output.WriteLine("1 flipped bit -> AuthenticationTagMismatchException (CBC in 6.6 had no such signal)");
        }
    }

    private void HashingAndHmac(string workDir)
    {
        Header("6.10 SHA-256 / SHA-512 / HMAC-SHA256 of a file");
        var file = Path.Combine(workDir, "report.txt");
        FileStreamBasics.WriteText(file, "Quarterly attendance report");
        output.WriteLine($"SHA-256: {Convert.ToHexString(FileHasher.Sha256File(file))}");
        output.WriteLine($"SHA-512: {Convert.ToHexString(FileHasher.Sha512File(file))[..64]}...");
        output.WriteLine($"HMAC k1: {Convert.ToHexString(FileHasher.HmacSha256File(file, "key-one"u8.ToArray()))}");
        output.WriteLine($"HMAC k2: {Convert.ToHexString(FileHasher.HmacSha256File(file, "key-two"u8.ToArray()))}");
    }

    private void ConstantTime()
    {
        Header("6.11 Constant-time compare");
        var digest = SHA256.HashData("x"u8);
        var other = SHA256.HashData("y"u8);
        output.WriteLine($"FixedTimeEquals(same)  = {DigestComparer.AreEqual(digest, digest.ToArray())}");
        output.WriteLine($"FixedTimeEquals(other) = {DigestComparer.AreEqual(digest, other)}");
    }

    private void Rsa()
    {
        Header("6.13 RSA-2048 OAEP-SHA256 (concept)");
        var (ciphertext, decrypted) = RsaDemo.RoundTrip("short secret");
        output.WriteLine($"public-key encrypt -> {ciphertext.Length}-byte ciphertext -> private-key decrypt: '{decrypted}'");
        output.WriteLine($"max plaintext per RSA operation: {RsaDemo.MaxPlaintextBytes()} bytes - bulk data needs hybrid RSA+AES");
    }

    private void Header(string title) => output.WriteLine($"{Environment.NewLine}== {title} ==");
}
