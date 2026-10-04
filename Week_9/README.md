# Week 9 - the tested full-stack Student Portal

```
student-portal-web (React, :5173)  --JWT-->  StudentApi (ASP.NET Core, :5095)  -->  SQL Server (EF Code First)
        Jest + RTL (103)                       xUnit + Moq (187)
                         \__________ Selenium E2E (6) drives both in Chrome __________/
```

| Folder | What |
|---|---|
| `StudentApi/` | The portal API (Weeks 5-8 rolled forward) plus a global exception handler (`ApiExceptionHandler`): any unhandled failure becomes a ProblemDetails 500 with no internals leaked |
| `StudentApi.Tests/` | Consolidated backend suite: Week 5 CRUD/status codes, Week 6 auth, Week 7 data layers, Week 8 registration/CORS, Week 9 `Canonical/` tests |
| `student-portal-web/` | The React client plus Week 9 resilience (401 → re-login, friendly 400/403/404/409/500, error boundary, one retry on a transient GET failure) |
| `StudentPortal.E2E/` | Selenium WebDriver (C#) suite with page objects |
| `run-all-tests.ps1` | Task 9.9: backend unit → frontend unit → E2E, with both coverage gates |

## Run everything (Task 9.9)

Prerequisites: .NET 10 SDK, Node 20+ (`nvm install lts` then `nvm use lts` if
using nvm-windows), SQL Server LocalDB, Chrome or Edge. Ports 5095 and 5173
must be free.

```
cd Week_9
powershell -ExecutionPolicy Bypass -File run-all-tests.ps1            # headless Chrome
powershell -ExecutionPolicy Bypass -File run-all-tests.ps1 -Headed    # watch it
powershell -ExecutionPolicy Bypass -File run-all-tests.ps1 -Browser edge
```

The script restores packages, applies the EF migrations, starts the API and
the React dev server, runs the three suites in order, and stops everything
again. Verified result:

```
=== Summary ===
PASS  Backend unit (coverage 98.9%)
PASS  Frontend unit (Jest coverage >= 80%)
PASS  E2E (Selenium)
ALL SUITES GREEN
```

## Run the portal by hand

```
dotnet tool restore
dotnet ef database update --project StudentApi --context AppDbContext
dotnet run --project StudentApi                       # http://localhost:5095
cd student-portal-web && npm install && npm run dev    # http://localhost:5173
```

Accounts: `teacher1` / `Teacher@123` (full CRUD), `student1` / `Student@123`
(read-only), or register a new one.

## What each test layer protects

| Layer | Count | Coverage | Protects | Can't catch |
|---|---|---|---|---|
| **Backend unit** (xUnit + Moq, mocked repositories; integration tests on SQLite / a throwaway LocalDB) | 187 | 98.9% lines | Business rules (409 duplicates, NotFound paths), auth (hashing, tokens, 401/403 per endpoint), each data layer's SQL/mapping, validation → 400, failures → clean 500 | Whether the React app calls the API correctly |
| **Frontend unit** (Jest + React Testing Library, axios mocked) | 103 | 100% lines / 95.8% branches | Rendering from props/state, role-based visibility, form validation, the token interceptor, error messages, 401 → re-login, retry | Whether the real API returns what the mocks pretend |
| **E2E** (Selenium, real Chrome → real API → real SQL Server) | 6 | not counted toward 80% | The joins: CORS, JWT round-trip, routing, real data | Fine-grained edge cases (too slow and brittle to test exhaustively here) |

Unit suites run first because they're fast and tell you exactly *which*
unit broke. E2E runs last because it's slow, and it's the only layer that
proves a real user can actually complete the journey.

### The canonical minimums

- Backend, in `StudentApi.Tests/Canonical/CanonicalServiceTests.cs`, all with the repository mocked:
  1. valid create
  2. missing id → NotFound / 404
  3. invalid input → 400, with the repository never touched
  4. simulated repository error → clean ProblemDetails 500, with no secret or stack trace in the body; a second test checks the Service doesn't swallow the error
  5. Student blocked on write (403, 0 inserts) / Teacher allowed (201, 1 insert)
- Frontend: role visibility in `StudentsPage.test.jsx` (Teacher sees Add/Edit/Delete, Student sees none).
- E2E: (1) register + login, (2) Teacher full CRUD, (3) Student read-only, (4) logout, (5) a **negative** test, plus a smoke test.
  The negative test runs inside a logged-in Student's browser tab and calls `POST /api/students` directly, bypassing
  the hidden button. The server answers **403** and nothing is written.

## Two real-browser lessons (fixed in `StudentPortal.E2E`)

- Chrome's password manager pops a "password found in a data breach" bubble after a form login with the demo
  passwords. It silently swallowed the next native click, so WebDriver reported success while the page got no event.
  The test browser profile now disables the password manager.
- `IWebElement.Clear()` empties an input behind React's back, so typing "88" into a cleared "55" produced "5588".
  The page objects use select-all + Delete instead (`ReplaceText`).
