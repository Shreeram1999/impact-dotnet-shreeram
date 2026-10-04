using System.Security.Cryptography;

namespace SecureFileVault.Integrity;

// Task 6.11 - why not == or SequenceEqual: both return as soon as they hit
// the first differing byte. Comparing an attacker's guessed HMAC against the
// real one therefore takes slightly longer the more leading bytes the guess
// gets right, and with enough repeated, timed attempts that difference
// reveals the correct digest one byte at a time (a timing side-channel).
// FixedTimeEquals always touches every byte, so the time taken says nothing
// about WHERE the first mismatch was. (A length mismatch returns false
// straight away - a digest's length isn't secret.)
public static class DigestComparer
{
    public static bool AreEqual(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right) =>
        CryptographicOperations.FixedTimeEquals(left, right);
}
