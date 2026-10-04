using System.Security.Cryptography;
using System.Text;

namespace SecureFileVault.Fundamentals;

// Task 6.2 - the three-way difference, in one line each:
//   Encoding   (Base64)  - reversible by ANYONE, no secret involved. It only
//                          changes representation (bytes -> safe text).
//                          WRONG tool for hiding anything: "encoded" data is
//                          as readable as plaintext to whoever decodes it.
//   Hashing    (SHA-256) - one-way and fixed-length (32 bytes whatever the
//                          input size). Proves "same input" without revealing
//                          it. WRONG tool when the data must come back out,
//                          and (unsalted + fast) WRONG for passwords - see
//                          PasswordKeyDeriver for why.
//   Encryption (AES)     - reversible, but ONLY with the key. The right tool
//                          when data must stay secret AND be recovered later.
//                          WRONG tool for passwords - a server should never be
//                          able to recover a user's password at all.
public static class EncodingVsHashing
{
    public static string ToBase64(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    public static string FromBase64(string base64) => Encoding.UTF8.GetString(Convert.FromBase64String(base64));

    public static string Sha256Hex(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}
