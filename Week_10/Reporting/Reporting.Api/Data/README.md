# Reporting data layer - EF Core Database First

`ReportingDbContext.cs` and everything in `Scaffolded/` are **generated** from
the reporting database (`../Database/*.sql`, owned by the data team) and
must not be edited by hand. When the data team changes the schema,
regenerate them (from `Week_10/Reporting/Reporting.Api`):

```
dotnet ef dbcontext scaffold "Server=(localdb)\MSSQLLocalDB;Database=StudentPortal_Reporting;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --output-dir Data/Scaffolded --context-dir Data --context ReportingDbContext --namespace Reporting.Api.Data.Scaffolded --context-namespace Reporting.Api.Data --no-onconfiguring --force
```

`--no-onconfiguring` keeps the connection string out of generated code (it
comes from `ConnectionStrings:ReportingDb`). There are no migrations in this
service: the database is the source of truth, and C# follows it.

Generated names are kept as EF produced them (e.g. `CourseCodeNavigation`),
because renaming them by hand would be overwritten on the next scaffold.
`Services/ReportService.cs` is the only code that touches these types, and
it exposes its own DTOs, so a schema change stops there.
