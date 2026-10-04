# DATA-COMPARISON (Task 7.12)

The same `IRepository<Student>` / `IRepository<Teacher>` / `IUserStore`
interfaces, implemented three ways in `StudentApi/Data/`, all passing the same
`smoke-tests.ps1` run (25 checks: CRUD, 200/201/204/400/401/403/404/409, search,
grade, SQL-injection literal) and switched by one setting:
`"DataLayer": { "Provider": "AdoNet" | "EfCodeFirst" | "EfDbFirst" }`.

## What I observed building each one

| | ADO.NET (`Data/AdoNet`) | EF Core Code First (`Data/EfCodeFirst`) | EF Core DB First (`Data/EfDbFirst`) |
|---|---|---|---|
| **Source of truth** | Hand-written SQL scripts (`Database/*.sql`) | The C# model + Fluent API in `AppDbContext` | The existing database; C# is generated from it |
| **Code I wrote for Student CRUD** | 140 lines (comments included): every SELECT/INSERT/UPDATE/DELETE, every parameter, every column mapped by hand | 75 lines for *all* entities (one generic `EfRepository<T>`) | 73 lines: CRUD plus a mapping between scaffolded `Db.Student` and the domain `Student` |
| **SQL I can see** | All of it, exactly as it runs, including the two stored procedures | Generated. I had to turn on EF logging to see it | Generated |
| **Schema change (Task 7.8 `EnrolledOn`)** | Edit the table script, both procs, the SELECT list, the parameters and the mapper. Five places, none checked by the compiler | Add one property, run `migrations add`, and the migration is generated, reviewable and reversible (`Down()`) | The owning team alters the table; I re-run `dbcontext scaffold` and adjust one mapping |
| **Injection safety** | Only as good as my discipline: every value is a `SqlParameter` (the test proves `'; DROP TABLE Students;--` is stored literally) | Parameterized by construction | Parameterized by construction |
| **Testing** | Needs a real SQL Server (throwaway DB built from the scripts) | Runs on SQLite in-memory in milliseconds, with constraints enforced | EF InMemory for the mapping; the schema belongs to the other team |
| **Surprises** | `QUOTED_IDENTIFIER` must be ON for filtered indexes from sqlcmd; DATE comes back as `DateTime` and needs converting to `DateOnly` | `HasData` needs constant values, so password hashes had to be precomputed strings; check constraints ride along into SQLite | The scaffold faithfully reproduced named constraints, the filtered unique indexes and the one-to-one relationships, without me describing any of them |

## Which approach fits which use case

### A stable security table (Users / login) → **ADO.NET**

The login table hardly ever changes shape (`Username`, `PasswordHash`,
`Role`), it sits on the most security-sensitive path in the system, and it
needs only two tiny queries. ADO.NET's verbosity costs almost nothing here,
and its explicitness is the benefit: `AdoNetUserStore.FindByUsername` is one
parameterized `SELECT` I can read in full and audit line by line. There's no
change tracker, lazy loading or generated SQL between a password check and
the database, and no ORM upgrade can change what runs. Stored procedures fit
too: a DBA can grant the API `EXECUTE` on a proc instead of table-level
permissions. Week 10's Identity service follows this reasoning.

### An evolving domain (Students, Teachers, Courses) → **EF Core Code First**

This is the part of the system we own and that changes every sprint. Adding
`EnrolledOn` showed the difference most clearly. In ADO.NET it meant editing
five places by hand with no compiler help, and missing one is a runtime bug.
In Code First it was one property plus `dotnet ef migrations add AddEnrolledOn`,
which produced a reviewable migration that also backfilled the seed rows and
can be rolled back. The model is the schema, relationships and unique indexes
are declared once in `AppDbContext`, and the same model runs on SQLite for fast
integration tests. Week 10's Academics service uses this.

### A reporting layer over a DB we don't own → **EF Core DB First**

When another team owns the schema, I don't get to write migrations against
it, so Code First is the wrong tool. Hand-writing ADO.NET for a wide
read-mostly reporting schema would be a lot of brittle mapping code.
`dotnet ef dbcontext scaffold` read the existing database and generated
correctly-typed entities, keys, unique indexes and relationships in one
command. When their schema changes, I re-scaffold. Keeping the scaffolded
classes behind my own mapping (`DbFirstStudentRepository`) means a column
rename on their side stops at one file. Week 10's Reporting service uses this.

## Honest caveat

Mixing three data-access strategies in one product is a teaching device. A
real team would normally standardise on one (usually EF Core, dropping to raw
ADO.NET/Dapper for the odd hot path) so that everyone reads, reviews and
debugs the same way. The point of doing all three is being able to argue
*why* each one fits where it does.
