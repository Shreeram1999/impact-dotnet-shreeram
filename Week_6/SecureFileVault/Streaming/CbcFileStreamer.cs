using System.Security.Cryptography;

namespace SecureFileVault.Streaming;

// Task 6.12 - stream-encrypt a file of any size by chaining
// FileStream -> CryptoStream, so no more than one buffer of it is ever in
// memory.
//
// CryptoStreamMode is about which side of the CryptoStream you talk to:
//   Encrypt: CryptoStream(output, encryptor, WRITE) - plaintext is WRITTEN
//            into it, ciphertext comes out the other side into the file.
//   Decrypt: CryptoStream(input, decryptor, READ) - plaintext is READ out of
//            it, pulling ciphertext from the file underneath on demand.
//
// Disposal order matters on the encrypt side: the CryptoStream must be
// disposed (which writes the final padded block) BEFORE the FileStream under
// it is closed. Nested usings dispose inner-first, which is exactly that
// order. Close the file first and the last block is silently lost, so the
// file can't be decrypted.
//
// This is the plain CBC version the task asks for. The vault's real format
// (Vault/SecureVault.cs) is chunked AES-GCM, because CBC has no tamper
// detection (Task 6.6).
public static class CbcFileStreamer
{
    public static void Encrypt(Stream plaintext, Stream ciphertextOutput, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        ciphertextOutput.Write(aes.IV);

        using var crypto = new CryptoStream(ciphertextOutput, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
        plaintext.CopyTo(crypto, 81920);
    }

    public static void Decrypt(Stream ciphertextInput, Stream plaintextOutput, byte[] key)
    {
        var iv = new byte[16];
        ciphertextInput.ReadExactly(iv);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var crypto = new CryptoStream(ciphertextInput, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true);
        crypto.CopyTo(plaintextOutput, 81920);
    }

    public static void EncryptFile(string inputPath, string outputPath, byte[] key)
    {
        using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920);
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920);
        Encrypt(input, output, key);
    }

    public static void DecryptFile(string inputPath, string outputPath, byte[] key)
    {
        using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920);
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920);
        Decrypt(input, output, key);
    }
}
