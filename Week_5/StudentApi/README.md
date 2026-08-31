# Student API (Week 5) - unsecured on purpose

An ASP.NET Core Web API promoting Week 4's console layering (Model / View /
Controller / Service / Repository) into real HTTP endpoints. No
authentication - that's intentional this week, per the syllabus.

## Running it

```
dotnet run --project StudentApi
```

By default (`Properties/launchSettings.json`, `http` profile) it listens on
`http://localhost:5095`.

- **Swagger UI:** http://localhost:5095/swagger/index.html - browse and
  exercise every endpoint interactively from here.
- **Raw OpenAPI document:** http://localhost:5095/swagger/v1/swagger.json
- **Request collection:** `StudentApi/StudentApi.http` - a full run-every-endpoint
  collection (success and failure cases for every verb) in VS Code's/Visual
  Studio's REST Client `.http` format, the same role a Postman collection
  would play. Every request in it was hand-verified against a running
  instance of this API while building it.

## Status-code map

| Verb | Route | Success | Failure |
|---|---|---|---|
| GET | `/api/students` | 200, list (possibly empty) | - |
| GET | `/api/students/{id}` | 200 | 404 if missing |
| GET | `/api/students/search?name=` | 200, matches or `[]` | - (empty query returns everyone, not an error) |
| GET | `/api/students/{id}/grade?scale=` | 200, `{ studentId, scale, grade }` | 404 if student missing |
| POST | `/api/students` | 201 + `Location` header + body | 400 (DataAnnotations, automatic) · 409 if roll number taken |
| PUT | `/api/students/{id}` | 204, no body | 400 (DataAnnotations) · 404 if missing · 409 if roll number taken by another student |
| DELETE | `/api/students/{id}` | 204 | 404 if missing |
| GET/POST/PUT/DELETE | `/api/teachers...` | same 200/201/204 shape as Students | 400 (DataAnnotations) · 404 if missing |

400s come entirely from `[ApiController]` + `StudentCreateDto`/`TeacherCreateDto`'s
`[Required]`/`[Range]`/`[EmailAddress]` attributes - no action method
contains a validation `if`. See `Validation_Notes.md` for what breaks
without `[ApiController]` (verified by actually removing it and re-testing,
not just described).

## Layer responsibility (same contract as Week 4, applied to HTTP)

- **Model** (`/Models`) - `Student`/`Teacher`, plus `InternalNotes` (Task
  5.7): an internal-only field no DTO ever exposes.
- **Data** (`/Data`) - `IRepository<T>`/`InMemoryRepository<T>`, unchanged
  in shape since Week 3, now `AddSingleton` and lock-protected for
  concurrent requests.
- **Service** (`/Services`) - `IStudentService`/`ITeacherService`: every
  business rule (duplicate roll numbers -> 409) and the applied
  Strategy/Factory pattern (`/Services/Grading`, Task 5.8 - see
  `Grading_Pattern_Notes.md`).
- **Dtos** (`/Dtos`) - the only shapes that cross the wire, plus the manual
  entity<->DTO mappers.
- **Controllers** (`/Controllers`) - orchestration only: call one Service
  method, map through a `StudentMapper`/`TeacherMapper`, pick a status code
  from the outcome. No business rule, no manual string formatting, no
  direct repository access anywhere in either controller.

## Applied design pattern

Strategy + Factory for grade display (`/Services/Grading`) - see
`Grading_Pattern_Notes.md` for the full reasoning, including why it beat
the response-formatter-factory alternative and why it's registered
`Singleton`.

## Tests

`StudentApi.Tests` - 63 xUnit tests, all passing:

- `Services/StudentServiceTests.cs`, `Services/TeacherServiceTests.cs` -
  the Service layer, `IRepository<T>` mocked with Moq.
- `Services/Grading/*Tests.cs` - each strategy's output, and the factory's
  selection logic (case-insensitivity, unknown-scale fallback).
- `Dtos/StudentMapperTests.cs` - proves `InternalNotes` never survives
  serialization of a mapped `StudentReadDto`.
- `Data/InMemoryRepositoryTests.cs` - the real repository implementation,
  including a concurrent-adds test for its lock.
- `Controllers/*Tests.cs` - full-pipeline integration tests via
  `WebApplicationFactory<Program>`, with the real `IStudentService`/`ITeacherService`
  swapped for an in-memory fake (`TestSupport/`), asserting the exact
  status codes in the map above - including the automatic 400 from
  `[ApiController]`, which only a real-pipeline test can actually exercise.

Coverage (coverlet.collector, `XPlat Code Coverage`): **96.5% overall line
coverage**; every class in `/Services` and `/Dtos` sits between 87.5% and
100%, comfortably clearing the ≥80% target the Testing Focus places on the
Service layer and the applied Strategy/Factory pattern.

## Stretch (Task 5.10)

`TeachersController` mirrors `StudentsController`'s CRUD/status-code shape
(minus search and grading, which don't apply to a Teacher) - both
controllers stay equally free of business logic.
