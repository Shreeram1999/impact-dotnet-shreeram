using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using StudentApi.Data.AdoNet;

namespace StudentApi.Tests.Data.AdoNet;

// Builds a throwaway SQL Server database from the REAL Database/*.sql
// scripts (schema, stored procedures, seed) for the ADO.NET tests, and drops
// it afterwards. Raw SQL can only be meaningfully tested against the engine
// that runs it, so these are integration tests by design.
//
// Server: STUDENTPORTAL_TEST_SQL environment variable, else LocalDB. If no
// server is reachable the tests are SKIPPED (reported as skipped, never as
// passed), so the suite still runs on a machine without SQL Server.
public sealed class SqlServerTestDatabase : IDisposable
{
    private static readonly string Server =
        Environment.GetEnvironmentVariable("STUDENTPORTAL_TEST_SQL") ?? @"(localdb)\MSSQLLocalDB";

    private readonly string databaseName = $"StudentPortal_Week7_Tests_{Guid.NewGuid():N}"[..40];

    public bool Available { get; }
    public string UnavailableReason { get; } = string.Empty;
    public ISqlConnectionFactory ConnectionFactory { get; } = null!;

    public SqlServerTestDatabase()
    {
        try
        {
            using (var master = new SqlConnection(ConnectionString("master")))
            {
                master.Open();
                Execute(master, $"CREATE DATABASE [{databaseName}];");
            }

            using (var database = new SqlConnection(ConnectionString(databaseName)))
            {
                database.Open();
                foreach (var script in new[] { "02_Schema.sql", "03_StoredProcedures.sql", "04_SeedData.sql" })
                    foreach (var batch in SplitOnGo(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Database", script))))
                        Execute(database, batch);
            }

            ConnectionFactory = new SqlConnectionFactory(ConnectionString(databaseName));
            Available = true;
        }
        catch (SqlException ex)
        {
            UnavailableReason = $"SQL Server not reachable at {Server}: {ex.Message}";
        }
    }

    public int ExecuteScalarInt(string sql)
    {
        using var connection = ConnectionFactory.Create();
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        return (int)command.ExecuteScalar()!;
    }

    public void Dispose()
    {
        if (!Available)
            return;

        SqlConnection.ClearAllPools();
        using var master = new SqlConnection(ConnectionString("master"));
        master.Open();
        Execute(master, $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];");
    }

    private static string ConnectionString(string database) =>
        $"Server={Server};Database={database};Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30";

    private static IEnumerable<string> SplitOnGo(string script) =>
        Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(batch => !string.IsNullOrWhiteSpace(batch));

    private static void Execute(SqlConnection connection, string sql)
    {
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}

[CollectionDefinition(Name)]
public class SqlServerCollection : ICollectionFixture<SqlServerTestDatabase>
{
    public const string Name = "SQL Server (ADO.NET)";
}
