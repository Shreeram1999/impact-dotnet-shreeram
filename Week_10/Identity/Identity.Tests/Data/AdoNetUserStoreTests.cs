using System.Text.RegularExpressions;
using Identity.Api.Auth;
using Identity.Api.Data;
using Identity.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentPortal.Shared;

namespace Identity.Tests.Data;

// The ADO.NET store against a throwaway SQL Server database built from the
// service's own scripts (01-03). Skipped - not passed - if no SQL Server is
// reachable (STUDENTPORTAL_TEST_SQL, default LocalDB).
public sealed class IdentityTestDatabase : IDisposable
{
    private static readonly string Server = Environment.GetEnvironmentVariable("STUDENTPORTAL_TEST_SQL") ?? @"(localdb)\MSSQLLocalDB";
    private readonly string name = $"IdentityTests_{Guid.NewGuid():N}"[..30];

    public bool Available { get; }
    public string Reason { get; } = string.Empty;
    public ISqlConnectionFactory Connections { get; } = null!;

    public IdentityTestDatabase()
    {
        try
        {
            Execute("master", $"CREATE DATABASE [{name}];");
            foreach (var script in new[] { "01_Schema.sql", "02_StoredProcedures.sql", "03_SeedData.sql", "01_Schema.sql", "03_SeedData.sql" })
                foreach (var batch in Regex.Split(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Database", script)), @"^\s*GO\s*$", RegexOptions.Multiline))
                    if (!string.IsNullOrWhiteSpace(batch))
                        Execute(name, batch);
            Connections = new SqlConnectionFactory(ConnectionString(name));
            Available = true;
        }
        catch (SqlException ex)
        {
            Reason = $"SQL Server not reachable at {Server}: {ex.Message}";
        }
    }

    public void Dispose()
    {
        if (!Available) return;
        SqlConnection.ClearAllPools();
        Execute("master", $"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}];");
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

public class AdoNetUserStoreTests : IClassFixture<IdentityTestDatabase>
{
    private readonly IdentityTestDatabase database;

    public AdoNetUserStoreTests(IdentityTestDatabase database)
    {
        this.database = database;
    }

    private AdoNetUserStore Store()
    {
        Skip.IfNot(database.Available, database.Reason);
        return new AdoNetUserStore(database.Connections);
    }

    // The scripts ran twice (01 and 03 are listed again in the fixture) -
    // idempotent, so still exactly one teacher1 and no error.
    [SkippableFact]
    public void SeededTeacher_IsFoundThroughTheStoredProcedure_AndItsHashVerifies()
    {
        var teacher = Store().FindByUsername("TEACHER1")!;

        Assert.Equal(Roles.Teacher, teacher.Role);
        Assert.Equal("Meera Iyer", teacher.DisplayName);
        Assert.Null(teacher.DateOfBirth);
        Assert.True(new Pbkdf2PasswordHasher().Verify("Teacher@123", teacher.PasswordHash));
    }

    [SkippableFact]
    public void Unknown_ReturnsNull()
    {
        Assert.Null(Store().FindByUsername("nobody"));
    }

    [SkippableFact]
    public void Add_RoundTripsEveryColumn_AndReturnsTheNewId()
    {
        var store = Store();
        var user = new User { Username = "ado.round@trip.example", PasswordHash = "h", Role = Roles.Student, DisplayName = "Round Trip", DateOfBirth = new DateOnly(2004, 2, 29) };

        store.Add(user);

        var loaded = store.FindByUsername(user.Username)!;
        Assert.True(user.Id > 2);
        Assert.Equal(user.Id, loaded.Id);
        Assert.Equal(new DateOnly(2004, 2, 29), loaded.DateOfBirth);
        Assert.Equal("Round Trip", loaded.DisplayName);
    }

    [SkippableFact]
    public void Add_WithoutOptionalColumns_StoresNulls()
    {
        var store = Store();

        store.Add(new User { Username = "no.extras@example", PasswordHash = "h", Role = Roles.Student });

        var loaded = store.FindByUsername("no.extras@example")!;
        Assert.Null(loaded.DisplayName);
        Assert.Null(loaded.DateOfBirth);
    }

    [SkippableFact]
    public void Add_DuplicateUsername_BecomesInvalidOperation()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Store().Add(new User { Username = "teacher1", PasswordHash = "h", Role = Roles.Teacher }));
    }

    [SkippableFact]
    public void InjectionAttempt_InTheUsername_IsJustData()
    {
        Assert.Null(Store().FindByUsername("' OR 1=1 --"));
    }

    [Fact]
    public void ConnectionFactory_RequiresTheConnectionString()
    {
        Assert.Throws<InvalidOperationException>(() => new SqlConnectionFactory(new ConfigurationBuilder().Build()));

        var configured = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:IdentityDb"] = "Server=x;Database=idb" })
            .Build();
        using var connection = new SqlConnectionFactory(configured).Create();
        Assert.Equal("idb", connection.Database);
    }
}
