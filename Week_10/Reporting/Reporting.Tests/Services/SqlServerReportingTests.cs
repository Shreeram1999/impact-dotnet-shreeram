using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Reporting.Api.Data;
using Reporting.Api.Services;

namespace Reporting.Tests.Services;

// The same report queries against REAL SQL Server, on a throwaway database
// built from the data team's own scripts - proving the LINQ GroupBy
// projections translate to T-SQL and the scaffolded model matches the real
// schema. Skipped (not passed) without SQL Server.
public sealed class SqlServerReportingTests : IDisposable
{
    private static readonly string Server = Environment.GetEnvironmentVariable("STUDENTPORTAL_TEST_SQL") ?? @"(localdb)\MSSQLLocalDB";
    private readonly string name = $"ReportingTests_{Guid.NewGuid():N}"[..31];
    private readonly bool available;
    private readonly string reason = string.Empty;

    public SqlServerReportingTests()
    {
        try
        {
            Execute("master", $"CREATE DATABASE [{name}];");
            foreach (var script in new[] { "01_ReportingSchema.sql", "02_ReportingSeedData.sql" })
                foreach (var batch in Regex.Split(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Database", script)), @"^\s*GO\s*$", RegexOptions.Multiline))
                    if (!string.IsNullOrWhiteSpace(batch))
                        Execute(name, batch);
            available = true;
        }
        catch (SqlException ex)
        {
            reason = $"SQL Server not reachable at {Server}: {ex.Message}";
        }
    }

    public void Dispose()
    {
        if (!available) return;
        SqlConnection.ClearAllPools();
        Execute("master", $"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}];");
    }

    [SkippableFact]
    public void Reports_RunOnSqlServer_AndMatchTheSqliteNumbers()
    {
        Skip.IfNot(available, reason);
        using var context = new ReportingDbContext(new DbContextOptionsBuilder<ReportingDbContext>().UseSqlServer(ConnectionString(name)).Options);
        var service = new ReportService(context);

        var monsoon = service.EnrollmentSummary(null)!;
        var spring = service.DepartmentSummary(1)!;

        Assert.Equal("Monsoon 2026", monsoon.Term.Name);
        Assert.Equal(new CourseSummaryDto("CS201", "Web APIs with ASP.NET Core", "Computer Science", 3, 4, 3, 70.3, 66.7),
            monsoon.Courses.Single(c => c.CourseCode == "CS201"));
        Assert.Equal(new DepartmentSummaryDto("Physics", 1, 3, 54.3), spring.Single(d => d.Department == "Physics"));
    }

    private static string ConnectionString(string database) =>
        $"Server={Server};Database={database};Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30";

    private static void Execute(string database, string sql)
    {
        using var connection = new SqlConnection(ConnectionString(database));
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}
