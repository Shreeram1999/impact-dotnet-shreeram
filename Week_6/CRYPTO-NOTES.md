# CRYPTO-NOTES (Week 6)

Write-ups for the Day 1, Day 3 and Day 5 "understand" tasks. The code for
every task lives in `SecureFileVault/` (run `dotnet run --project SecureFileVault -- demo`
to see each one's evidence printed).

## Day 1

### Task 6.2 - encoding vs hashing vs encryption

| | Reversible? | Needs a secret? | Output size | Wrong tool when... |
|---|---|---|---|---|
| **Encoding** (Base64) | Yes, by anyone | No | Grows ~33% | ...you want to hide anything. Base64 is just a different alphabet. |
| **Hashing** (SHA-256) | No | No | Fixed (32 bytes) | ...the data must come back out, or for passwords (fast + unsalted = brute-forceable). |
| **Encryption** (AES) | Yes, with the key | Yes | ≈ input (+ IV/tag) | ...storing passwords. The server should never be able to recover them. |

One line: **encoding changes representation, hashing proves identity, encryption hides content.**

### Task 6.3 - block cipher modes

- **ECB** encrypts every 16-byte block independently with the same key, so
  it is *deterministic per block*: identical plaintext blocks give identical
  ciphertext blocks, and patterns (the famous ECB penguin) show straight
  through. Never use it.
- **CBC** XORs each block with the previous ciphertext block (the first
  with a random IV), which hides patterns. It needs padding and gives **no
  integrity**: a wrong key or a tampered byte is only noticed if the padding
  breaks (Task 6.6), and padding errors can become a padding oracle.
- **CTR** turns AES into a stream cipher by encrypting a counter and XORing
  the result with the plaintext. It needs no padding and blocks can be
  processed in parallel, but there is still no integrity, and reusing a
  nonce is catastrophic.
- **GCM** = CTR + a GMAC authentication tag: it is **authenticated
  encryption**. Decrypt verifies the tag first and refuses any modified
  byte (Task 6.9). This is the default choice, and what the vault uses.

## Day 3

### Task 6.7 - why salt + iterations

- **Salt** (16 random bytes, stored next to the ciphertext/hash): makes the
  same password derive a different key every time, so precomputed
  password→key tables (rainbow tables) are useless and two users with the
  same password don't share a hash.
- **Iterations** (100,000 rounds of HMAC-SHA256): a legitimate user pays
  this cost once per login or file and never notices it. An attacker
  guessing offline pays it **per guess**, which cuts their guess rate by
  roughly 100,000x.

### Task 6.9 - GCM vs CBC under tampering

| | Wrong key | One flipped ciphertext bit |
|---|---|---|
| **AES-CBC** (6.6) | `CryptographicException` *most* of the time (bad padding). About 1 time in 256 it returns garbage silently. | Silently corrupts that block and flips one bit of the next. No error at all unless the padding breaks. |
| **AES-GCM** (6.9) | Always `AuthenticationTagMismatchException` | Always `AuthenticationTagMismatchException`, and no plaintext is returned |

CBC gives confidentiality only. GCM's tag gives integrity as well, which is
what CBC couldn't do.

### The vault's streaming format (Task 6.12 + deliverable)

`AesGcm` is one-shot, so `SecureVault` seals the file in 1 MB chunks, each
with its own nonce (`random 8-byte prefix || chunk index`) and tag. The
header, the chunk index and an "is final" flag are bound in as associated
data, so chunks can't be reordered, swapped between files, or silently
dropped from the end. Verified on a 120 MB file: byte-identical round-trip,
peak working set 27 MB, and a single flipped bit rejected (exit code 2) with
the partial output deleted.

## Day 5

### Task 6.13 - RSA, OAEP and hybrid encryption

- **Encrypt with the public key, decrypt with the private key** gives
  confidentiality: anyone can send me a secret. **Sign with the private
  key, verify with the public key** gives authenticity: only I could have
  produced the signature.
- RSA-2048 with OAEP-SHA256 can encrypt at most **190 bytes** per
  operation (256 − 2×32 − 2), and it's far slower than AES. **RSA can't do
  bulk data.**
- **Hybrid RSA+AES**: generate a random AES key, encrypt the data with
  AES-GCM, and encrypt only the 32-byte AES key with the recipient's RSA
  public key. TLS uses the same split so two strangers can agree a fast
  symmetric key over an open network. TLS 1.3 does the key agreement with
  ephemeral Diffie-Hellman and uses RSA/ECDSA to sign the handshake.

### Task 6.14 - authentication vs authorization

**Authentication** answers *who are you?* It verifies an identity claim
against something only the real user has, such as a password checked
against its PBKDF2 hash. Example: `POST /api/auth/login` with
`teacher1` / `Teacher@123` succeeds and returns a token; the wrong password
gets 401. **Authorization** answers *what may you do?*, given an identity
that is already established. Example: `student1` is fully authenticated, but
`POST /api/students` still returns **403** because that endpoint requires the
Teacher role. A failed authentication is a 401; a failed authorization is a
403. Both must be enforced on the server. A hidden button in the UI protects
nothing, because anyone can send the HTTP request directly.

**Session/cookie vs token.** With a server-side **session**, the server stores
"session 8f3a… = teacher1" and the browser just carries the opaque session id
in a cookie, which the browser sends automatically. Logging out deletes the
server record, and revocation is instant. Example: a classic MVC app with
ASP.NET Core cookie authentication. With a **token** (a JWT), the server
stores nothing. The signed token itself carries `sub=teacher1, role=Teacher,
exp=…`, and the client attaches it to each request as
`Authorization: Bearer …`. Any server holding the key can verify it, which
suits APIs, SPAs and microservices (Week 10's gateway). The cost is that a
stolen token stays valid until it expires, so lifetimes are kept short.
Example: this week's Student API, whose login returns a 60-minute JWT.
