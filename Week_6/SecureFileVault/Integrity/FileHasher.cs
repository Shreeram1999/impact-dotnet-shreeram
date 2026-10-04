using System.Security.Cryptography;

namespace SecureFileVault.Integrity;

// Task 6.10 - hash vs HMAC.
//   SHA-256 / SHA-512 prove WHAT: "this file is byte-for-byte the one whose
//   digest I published". But anyone can recompute a plain hash, so an
//   attacker who can swap the file can just as easily swap the digest next
//   to it.
//   HMAC-SHA256 proves WHAT and WHO: the digest is keyed, so only someone
//   holding the key can produce a matching value. Change the key and the
//   HMAC changes completely, even though the file didn't.
// All three read the stream incrementally (HashData(Stream) buffers
// internally), so hashing a 100 MB file never loads it into memory.
public static class FileHasher
{
    public static byte[] Sha256(Stream stream) => SHA256.HashData(stream);

    public static byte[] Sha512(Stream stream) => SHA512.HashData(stream);

    public static byte[] HmacSha256(Stream stream, byte[] key) => HMACSHA256.HashData(key, stream);

    public static byte[] Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Sha256(stream);
    }

    public static byte[] Sha512File(string path)
    {
        using var stream = File.OpenRead(path);
        return Sha512(stream);
    }

    public static byte[] HmacSha256File(string path, byte[] key)
    {
        using var stream = File.OpenRead(path);
        return HmacSha256(stream, key);
    }
}
