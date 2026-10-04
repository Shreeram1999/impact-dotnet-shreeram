# Student Portal - microservices architecture (Week 10)

```
                    React SPA (Vite build served by nginx, :3000 in Docker / :5173 locally)
                                   |
                                   |  every call: Authorization: Bearer <JWT issued by Identity>
                                   v
              +--------------------------------------------------+
              |        API Gateway - YARP (:8080 / :5100)        |
              |  - validates the JWT once (issuer, audience,     |
              |    signature, expiry) -> 401 at the edge         |
              |  - strips client X-User-* headers, forwards the  |
              |    validated sub/role/uid as X-User-* headers    |
              |  - CORS for the SPA origin                       |
              |  - /identity/* (anonymous)  /academics/*  /reporting/*  (JWT required)
              +-----------+-------------------+------------------+
                          |                   |                  |
                          v                   v                  v
              +---------------+     +-------------------+    +----------------------+
              |   Identity    |     |    Academics      |    |     Reporting        |
              |   ADO.NET     |     |  EF Core          |    |  EF Core             |
              |  + stored     |     |  Code First       |    |  Database First      |
              |  procedures   |     |  + migrations     |    |  (scaffolded)        |
              +-------+-------+     +---------+---------+    +----------+-----------+
                      |                       |                         |
                      v                       v                         v
         StudentPortal_Identity    StudentPortal_Academics    StudentPortal_Reporting
         Users (login, PBKDF2)     Students, Teachers,        Terms, Departments,
                                   Courses, Enrollments       CourseCatalog, EnrollmentFacts
                                   (evolving domain)          (pre-existing, data team's)
```

- **Database per service.** Each service has its own database and connection string, and no service ever connects
  to another's database. In Docker the three databases share one SQL Server container for convenience, but they're
  still separate databases owned by separate services. A consequence: Academics' `Student` no longer has a `UserId`
  foreign key, because a database can't reference a table in another service's database.
- **One token, many verifiers.** Identity is the only service that knows passwords and the only one that signs JWTs.
  The gateway, Academics and Reporting verify them with identical rules from `Shared/StudentPortal.Shared`.
- **Defence in depth.** The gateway is the front door, but Academics and Reporting still validate the JWT themselves
  and enforce `[Authorize]` / `[Authorize(Roles = "Teacher")]`. A request that somehow skipped the gateway still meets
  the same checks. The forwarded `X-User-*` headers are only used for logging and auditing, never for access decisions.
- **Failure isolation (Task 10.10).** If Reporting is down, the gateway answers 502 for `/reporting/*` only. The React
  Reports page says "Reporting is temporarily unavailable", while login (Identity) and students/courses (Academics)
  keep working. `run-all-tests.ps1` stops Reporting mid-run and asserts exactly that.

## Why each service uses the data-access approach it does (Task 10.9)

| Service | Approach | Why it fits |
|---|---|---|
| **Identity**: register, login, PBKDF2 hashing, JWT issuing (promoted from Week 6) | **ADO.NET + stored procedures** | It's the most security-critical code in the system, and the schema is small and stable (one `Users` table that has barely changed since Week 7). Raw ADO.NET keeps every query explicit and auditable: the service calls exactly two stored procedures (`usp_GetUserByUsername`, `usp_InsertUser`) with typed parameters, and contains no SQL text at all. There's no ORM, change tracker or generated SQL on the login path, and in production the service's SQL login would get `EXECUTE` on those two procedures and nothing else. ADO.NET's main cost, hand-mapping every column, is tiny for a five-column table. |
| **Academics**: Students, Teachers, Courses, Enrollments | **EF Core Code First** | This is the domain we own and keep changing: Week 7 added `EnrolledOn`, Week 10 added Courses and the many-to-many Enrollments. With Code First the C# model is the schema. A change is a property plus `dotnet ef migrations add`, which produces a reviewable, reversible migration; relationships, cascades and unique indexes are declared once in `AcademicsDbContext`; and the same model runs on SQLite in-memory, which is why the Academics tests are fast and still enforce real constraints. |
| **Reporting**: read-mostly enrollment summaries | **EF Core Database First** | The reporting database belongs to another team (the data team's ETL fills it), so we must not write migrations against it. `dotnet ef dbcontext scaffold` reads their schema and generates the entities and context, keys and relationships included. When they change the schema, we re-scaffold. Our hand-written code (`ReportService`) maps scaffolded types to our own DTOs, so their changes stop at one file. Reporting is integration, not greenfield. |

## Honest framing

**A real product would usually standardise on ONE data-access strategy**, typically EF Core, dropping to raw
ADO.NET or Dapper only for a measured hot path. One approach means one set of conventions to learn, review, test and
debug, and engineers can move between services without switching mental models. This portal mixes three on purpose,
as a training exercise, so that each one sits where its strengths are easiest to see. The table above is the
argument for *why* each fits its service. It is not a recommendation to mix strategies in production.

## Trade-offs I'd revisit in a real deployment

- **HS256 shared key → RS256.** With a shared HMAC key, every service that can verify a token could also mint one.
  Signing with a private key that only Identity holds (and publishing the public key via JWKS) removes that risk.
- **One SQL Server container → separate servers/credentials.** Today all three connection strings use `sa`. Each
  service should have its own login, with rights only on its own database.
- **Reporting's data flow.** Here the "ETL" is a seed script. In reality, Academics changes would reach the warehouse
  through events (a message broker) or a scheduled extract, never through a cross-service database join.
