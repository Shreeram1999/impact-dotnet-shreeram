# Student API (Week 6) - the minimal auth slice

The Week 5 Student API (see `Week_5/StudentApi/README.md` for the CRUD,
status codes, DTOs and the Strategy/Factory grade endpoint), plus Task 6.15:
PBKDF2-hashed users, a JWT login, and one Teacher-only write endpoint
enforced on the server.

## Running it

```
dotnet run --project StudentApi
```

- Swagger UI: http://localhost:5095/swagger/index.html. Log in with
  `POST /api/auth/login`, click **Authorize**, and paste the `token`.
- Request collection: `StudentApi.http`. Run the two logins at the top first;
  later requests reuse their tokens.

## Demo accounts (seeded in memory at startup, stored only as PBKDF2 hashes)

| Username | Password | Role |
|---|---|---|
| `teacher1` | `Teacher@123` | Teacher |
| `student1` | `Student@123` | Student |

## Auth endpoints and the role check

| Request | Result |
|---|---|
| `POST /api/auth/login` (right password) | 200 `{ token, expiresAtUtc, username, role }` |
| `POST /api/auth/login` (wrong password *or* unknown user) | 401 (same response, so usernames can't be discovered) |
| `GET /api/auth/me` | 200 `{ username, role }` with a token, 401 without |
| `POST /api/students` - no token | **401** |
| `POST /api/students` - Student token | **403** |
| `POST /api/students` - Teacher token | **201** |
| `POST /api/students` - tampered token | **401** (signature check fails) |
| any `GET` | anonymous, unchanged from Week 5 |

Verified against the running app with curl:

```
login wrong pw  -> 401
POST no token   -> 401
POST student    -> 403
POST teacher    -> 201
GET /me teacher -> {"username":"teacher1","role":"Teacher"}
tampered token  -> 401
payload: {"aud":"StudentPortal","iss":"StudentApi","exp":...,"sub":"student1","role":"Student","uid":"2"}
```

The decoded payload above shows the token is **signed, not encrypted**:
anyone can read the claims, but they can't change them without the signing
key. That's why the token carries no password or personal data.

## What was added (all new code is under `Auth/` plus three files)

- `Auth/Pbkdf2PasswordHasher.cs` - `PBKDF2-SHA256$100000$salt$hash`,
  verified with `CryptographicOperations.FixedTimeEquals`.
- `Auth/IUserStore.cs` / `InMemoryUserStore.cs` - the seam Week 7 swaps for SQL Server.
- `Auth/JwtTokenService.cs` / `JwtOptions.cs` - HMAC-SHA256 JWT with `sub`, `role`, `uid`, `exp`.
- `Services/AuthService.cs` - the login rule (find, verify, issue).
- `Controllers/AuthController.cs` - `login` and `me`, as thin as `StudentsController`.
- `Program.cs` - `AddAuthentication().AddJwtBearer()`, with `UseAuthentication()` *before* `UseAuthorization()`.
- `StudentsController.Create` - `[Authorize(Roles = Roles.Teacher)]`.

The signing key is in `appsettings.Development.json` for local runs only.
Outside development it must come from user-secrets or the `Jwt__Key`
environment variable. Startup fails fast (`ValidateOnStart`) if it's missing
or shorter than 32 characters.

## Tests

`StudentApi.Tests` has 101 tests, all passing, with 97.7% line coverage. That
is the 63 Week 5 tests (write requests now send a Teacher token) plus:
password hashing (right/wrong/malformed/salted), token claims, expiry and
signature, the user store, `AuthService` with every dependency mocked, and
end-to-end 401/403/201 checks through the real pipeline.
