# SecureFileVault (Week 6)

A console tool that encrypts files of any size with a password:
**password → PBKDF2 (100,000 × HMAC-SHA256, random salt) → AES-256-GCM**,
streamed in 1 MB chunks so a 100 MB+ file never sits in memory, and
rejecting any tampered byte.

## Commands

```
dotnet run --project SecureFileVault -- demo                      # every Task 6.1-6.13 demo
dotnet run --project SecureFileVault -- generate big.bin 120      # 120 MB random file
dotnet run --project SecureFileVault -- encrypt big.bin big.sfv   # prompts for a password
dotnet run --project SecureFileVault -- decrypt big.sfv big.out
dotnet run --project SecureFileVault -- compare big.bin big.out   # constant-time digest compare
dotnet run --project SecureFileVault -- tamper big.sfv 60000000   # flip one bit
dotnet run --project SecureFileVault -- decrypt big.sfv big2.out  # -> REJECTED, exit code 2
dotnet run --project SecureFileVault -- stream-demo out 110       # Task 6.12 CBC CryptoStream round-trip
```

The password can also come from `--password <p>` or the `SFV_PASSWORD`
environment variable (handy for scripts, but it shows up in shell history).

## Verified run (120 MB)

```
Generated big.bin (125,829,120 bytes).
Encrypted big.bin -> big.sfv (peak working set 27 MB).
Decrypted and verified big.sfv -> big.out (peak working set 27 MB).
IDENTICAL (SHA-256 digests match)
Flipped 1 bit at offset 60000000 of big.sfv.
REJECTED: wrong password, or the file has been tampered with. No output was kept.
```

## Layout

| Folder | Task | What |
|---|---|---|
| `FileHandling/` | 6.1 | FileStream / StreamReader / StreamWriter, 4 KB chunked copy |
| `Fundamentals/` | 6.2 | Base64 vs SHA-256 |
| `Symmetric/` | 6.4-6.6 | AES-CBC through CryptoStream, IV rule, wrong-key behaviour |
| `KeyDerivation/` | 6.7 | PBKDF2 key derivation |
| `Authenticated/` | 6.8-6.9 | AES-GCM blob `salt‖nonce‖tag‖ciphertext`, tamper rejection |
| `Integrity/` | 6.10-6.11 | SHA-256/512, HMAC-SHA256, `FixedTimeEquals` |
| `Streaming/` | 6.12 | FileStream → CryptoStream, large-file generator |
| `Vault/` | deliverable | chunked, authenticated streaming format (see `CRYPTO-NOTES.md`) |
| `Asymmetric/` | 6.13 | RSA-2048 OAEP-SHA256 concept demo |

Tests: `SecureFileVault.Tests` (106 tests, 96.6% line coverage).
