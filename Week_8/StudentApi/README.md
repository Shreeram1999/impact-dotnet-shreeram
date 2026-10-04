# Student API (Week 8) - ready for the React client

The Week 7 API (three swappable data layers, PBKDF2 + JWT auth) with what
the SPA in `../student-portal-web` needs:

| Change | Where |
|---|---|
| `POST /api/auth/register` (Name, DateOfBirth, Designation, Email, Password) → 201 / 400 / 403 / 409 | `AuthController`, `AuthService.Register`, `RegisterRequestDto` |
| Users gain `DisplayName` + `DateOfBirth` | EF migration `AddUserProfile`; `../Database/05_AddUserProfile.sql` + DB First re-scaffold; ADO.NET store SQL |
| JWT carries a `name` claim; `/api/auth/me` returns it | `JwtTokenService`, `CurrentUserDto` |
| **Every** write endpoint is Teacher-only (students POST/PUT/DELETE, teachers POST/PUT/DELETE) | `[Authorize(Roles = Roles.Teacher)]` |
| CORS for the SPA origin, tightened to 4 verbs + 2 headers | `Program.cs` (`Cors:AllowedOrigins`) |
| Teacher self-registration behind a flag (off by default, on in Development) | `RegistrationOptions` |

## Setup

Same as Week 7 but with Week 8 databases (from the `Week_8` folder):

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database/01_CreateDatabase.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -d StudentPortal_Week8 -i Database/02_Schema.sql -i Database/03_StoredProcedures.sql -i Database/04_SeedData.sql -i Database/05_AddUserProfile.sql
dotnet tool restore
dotnet ef database update --project StudentApi --context AppDbContext
dotnet run --project StudentApi
```

## CORS before/after (Task 8.9)

```
== BEFORE (Cors:AllowedOrigins empty) - preflight from http://localhost:5173
HTTP/1.1 204 No Content                       <- no Access-Control-Allow-Origin: the browser blocks the call
== AFTER (Cors:AllowedOrigins = http://localhost:5173)
HTTP/1.1 204 No Content
Access-Control-Allow-Origin: http://localhost:5173
```

## Tests

`StudentApi.Tests` has 180 tests, all passing, with 98.8% line coverage. New
this week: registration service rules (mocked store, pinned clock),
registration end to end (201 then login, 409, weak password, bad
designation/email, future DOB, Teacher 201 in Development / 403 when
disabled), 401/403 on every write endpoint, CORS preflight (allowed origin,
unknown origin, unadvertised verb) and the new profile columns in all three
data layers.
