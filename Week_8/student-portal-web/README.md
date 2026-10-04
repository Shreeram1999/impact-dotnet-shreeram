# Student Portal - React client (Week 8)

A Vite + React single-page app for the Week 8 Student API: register, log
in, keep the JWT in memory, attach it to every request with an axios
interceptor, list and search students, and show Create/Edit/Delete only to
Teachers.

## Run it

```
# terminal 1 - the API (from Week_8)
dotnet run --project StudentApi            # http://localhost:5095, EF Code First by default

# terminal 2 - the client
cd student-portal-web
npm install
npm run dev                                # http://localhost:5173
```

Log in as `teacher1` / `Teacher@123` (sees Add/Edit/Delete) or `student1` /
`Student@123` (read-only), or register a new account. The API URL defaults
to `http://localhost:5095`; set `VITE_API_BASE_URL` to change it.

## Tests

```
npm test                  # Jest + React Testing Library
npm run test:coverage     # fails if lines/branches/functions/statements < 80%
```

87 tests across 7 suites, all passing. Coverage: **100% statements, 96.3% branches,
100% functions, 100% lines**. The threshold is enforced in `jest.config.cjs`.

| Suite | What it proves |
|---|---|
| `StudentList.test.jsx` | rows render from props; empty state; Actions column only when editable; search box reports keystrokes and counts |
| `StudentsPage.test.jsx` | useEffect load with loading/empty/error states; live search + counter; **Student sees no write controls, Teacher sees Add/Edit/Delete**; create/edit/delete flows incl. a 409 shown in the form |
| `api.test.js` | **the interceptor attaches `Authorization: Bearer <token>`** (and nothing when logged out); wrappers hit the right verb/route; friendly error messages per status |
| `auth.test.jsx` | role read from the JWT (UTF-8 safe); login/logout update context + interceptor token; ProtectedRoute redirects to /login without a token |
| `pages.test.jsx` | **controlled forms keep Submit disabled until valid**; inline errors; login redirect; registration success/409/403 |
| `validation.test.js` | every client-side rule and boundary |
| `App.test.jsx` | routing: unknown URL → login, login → list → logout |

## Structure

```
src/
  api/        client.js (axios instance + interceptor), authApi, studentsApi, errors
  auth/       AuthContext (in-memory session), ProtectedRoute, jwt (claim decoding)
  components/ StudentRow, StudentList, SearchBox, StudentForm, NavBar
  pages/      LoginPage, RegisterPage, StudentsPage
  validation.js   client copies of the API's rules (UX only - the API re-checks)
```

Security notes: the token lives only in memory (a refresh logs you out, but
an injected script can't lift it from localStorage). Hiding buttons from
Students is a convenience; the API returns 403 for any Student write
regardless of the UI.
