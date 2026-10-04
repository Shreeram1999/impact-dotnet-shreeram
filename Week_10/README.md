# Week 10 - Microservices capstone: portal assembly & demo

The Weeks 5-9 portal split into independently deployable services behind a
YARP gateway, each owning its database and using the data-access approach
that fits it. See **[ARCHITECTURE.md](ARCHITECTURE.md)** for the diagram and the reasoning behind each database choice.

| Folder | What | Tests (coverage) |
|---|---|---|
| `Shared/StudentPortal.Shared` | JWT validation rules + claim names + ProblemDetails handler, shared by all services | measured inside each service |
| `Identity/` | Register / login / me. ADO.NET + stored procedures over `StudentPortal_Identity` | `Identity.Tests`: 46 (100%) |
| `Academics/` | Students, Teachers, Courses, Enrollments. EF Code First + migration over `StudentPortal_Academics` | `Academics.Tests`: 66 (96.2%) |
| `Reporting/` | Read-only enrollment summaries. EF DB First scaffolded from `StudentPortal_Reporting` | `Reporting.Tests`: 17 (100%) |
| `Gateway/` | YARP: routing, central JWT validation, claim forwarding, CORS | `Gateway.Tests`: 15 (92.8%) |
| `web/student-portal-web` | React client (Students + Reports pages), talks only to the gateway | Jest: 115 (100% lines) |
| `e2e/StudentPortal.E2E` | Selenium journeys through the gateway | 7 E2E |
| `docker-compose.yml`, `docker/` | The whole stack in containers | |

## Run it with Docker (Task 10.6 / 10.8)

```
cd Week_10
docker compose up --build
```

- React: http://localhost:3000. Gateway: http://localhost:8080 (the only published API port).
- `sqlserver` starts → `db-init` runs the Identity and Reporting scripts → `identity`, `reporting` start. `academics`
  applies its own EF migrations on start (`Database__MigrateOnStartup=true`).
- Log in as `teacher1` / `Teacher@123` or `student1` / `Student@123`. Teacher self-registration is off in this
  configuration; anyone can register as a Student.
- Optional `.env` next to the compose file: `SA_PASSWORD=...` and `JWT_KEY=...` (at least 32 characters).
- E2E against the containers:
  `set E2E_WEB_URL=http://localhost:3000 & set E2E_API_URL=http://localhost:8080 & dotnet test e2e/StudentPortal.E2E`
- Task 10.10 live: `docker compose stop reporting`. The Reports page then shows "temporarily unavailable" while
  Students keeps working. `docker compose start reporting` brings it back.

> Verification status: `docker compose config` validates. Each service was also published in Release and run in
> Production mode with exactly the environment variables compose supplies. All four started, Academics migrated a
> fresh database, Teacher self-registration answered 403, and a missing `Jwt__Key` failed fast. The images were
> **not** built and run here, because the Docker daemon wasn't running on the development machine.

## Run it locally without Docker

```
cd Week_10
powershell -ExecutionPolicy Bypass -File setup-local-databases.ps1     # LocalDB: 3 databases
dotnet run --project Identity/Identity.Api      # :5101
dotnet run --project Academics/Academics.Api    # :5102
dotnet run --project Reporting/Reporting.Api    # :5103
dotnet run --project Gateway/Gateway.Api        # :5100
cd web/student-portal-web && npm install && npm run dev               # :5173
```

## Test everything (Task 10.7)

```
powershell -ExecutionPolicy Bypass -File run-all-tests.ps1
```

Each service's suite is gated at ≥ 80%, then the Jest suite runs, then the composed stack comes up for Selenium,
then Reporting is stopped to prove degraded mode. Verified result:

```
PASS  Identity (ADO.NET) unit (coverage 100%)
PASS  Academics (EF Code First) unit (coverage 96.2%)
PASS  Reporting (EF DB First) unit (coverage 100%)
PASS  Gateway (YARP) unit (coverage 92.8%)
PASS  Frontend unit (Jest coverage >= 80%)
PASS  E2E (Selenium, through the gateway)
PASS  Degraded mode (Reporting down, rest works)
ALL SUITES GREEN
```

### The gateway-level test

`Gateway.Tests/GatewayTests.cs` → `TamperedJwt_IsRejectedAtTheGateway_AndNeverReachesAService`: one character of a
valid token's signature is changed, the gateway answers **401**, and a real stub service behind it records **zero**
requests. Related tests cover a forged "Teacher" payload, wrong issuer/key, an expired token, spoofed `X-User-Role`
headers being replaced with the token's real claims, and one service down → 502 for that route only.

## API through the gateway

| Gateway path | Service | Auth |
|---|---|---|
| `POST /identity/api/auth/login`, `POST /identity/api/auth/register` | Identity | anonymous |
| `GET /identity/api/auth/me` | Identity | JWT |
| `/academics/api/students[...]`, `/academics/api/teachers[...]`, `/academics/api/courses[...]` | Academics | JWT; writes Teacher-only |
| `POST /academics/api/courses/{id}/enrollments`, `DELETE .../enrollments/{studentId}`, `GET .../students` | Academics | JWT; writes Teacher-only |
| `GET /reporting/api/reports/terms`, `/enrollment-summary?termId=`, `/departments?termId=` | Reporting | JWT |
