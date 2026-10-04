using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Reporting.Api.Data;
using Reporting.Api.Data.Scaffolded;

namespace Reporting.Tests.TestSupport;

// A SQLite in-memory copy of the reporting database: the SCAFFOLDED model
// creates the tables, and the rows mirror Database/02_ReportingSeedData.sql
// exactly, so expected numbers here match what the real database returns.
public sealed class ReportingTestDatabase : IDisposable
{
    public SqliteConnection Connection { get; } = new("DataSource=:memory:");

    public ReportingTestDatabase(bool seed = true)
    {
        Connection.Open();
        using var context = CreateContext();
        context.Database.EnsureCreated();
        if (seed)
            Seed(context);
    }

    public ReportingDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ReportingDbContext>().UseSqlite(Connection).Options);

    public void Dispose() => Connection.Dispose();

    private static void Seed(ReportingDbContext context)
    {
        context.Terms.AddRange(
            new Term { TermId = 1, Name = "Spring 2026", StartsOn = new DateOnly(2026, 1, 5), EndsOn = new DateOnly(2026, 5, 15) },
            new Term { TermId = 2, Name = "Monsoon 2026", StartsOn = new DateOnly(2026, 6, 1), EndsOn = new DateOnly(2026, 11, 20) });
        context.Departments.AddRange(
            new Department { DepartmentId = 1, Name = "Mathematics" },
            new Department { DepartmentId = 2, Name = "Physics" },
            new Department { DepartmentId = 3, Name = "Computer Science" });
        context.CourseCatalogs.AddRange(
            new CourseCatalog { CourseCode = "MATH101", Title = "Calculus I", DepartmentId = 1, Credits = 4 },
            new CourseCatalog { CourseCode = "MATH201", Title = "Linear Algebra", DepartmentId = 1, Credits = 3 },
            new CourseCatalog { CourseCode = "PHYS101", Title = "Mechanics", DepartmentId = 2, Credits = 4 },
            new CourseCatalog { CourseCode = "CS101", Title = "Programming in C#", DepartmentId = 3, Credits = 3 },
            new CourseCatalog { CourseCode = "CS201", Title = "Web APIs with ASP.NET Core", DepartmentId = 3, Credits = 3 });

        (int Term, string Course, string Roll, int? Score)[] facts =
        [
            (1, "MATH101", "R001", 78), (1, "MATH101", "R002", 35), (1, "MATH101", "R003", 92), (1, "MATH101", "R004", 61),
            (1, "PHYS101", "R001", 70), (1, "PHYS101", "R002", 55), (1, "PHYS101", "R005", 38),
            (1, "CS101", "R003", 95), (1, "CS101", "R004", 81), (1, "CS101", "R005", 67), (1, "CS101", "R006", 74),
            (2, "MATH201", "R001", null), (2, "MATH201", "R003", 88), (2, "MATH201", "R004", null),
            (2, "PHYS101", "R002", 64), (2, "PHYS101", "R006", null),
            (2, "CS201", "R001", 82), (2, "CS201", "R003", 90), (2, "CS201", "R005", null), (2, "CS201", "R006", 39)
        ];
        context.EnrollmentFacts.AddRange(facts.Select(f => new EnrollmentFact
        {
            TermId = f.Term, CourseCode = f.Course, StudentRollNumber = f.Roll, FinalScore = f.Score, Completed = f.Score is not null
        }));
        context.SaveChanges();
    }
}
