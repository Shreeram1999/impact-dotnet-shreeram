using Microsoft.Data.SqlClient;

namespace StudentApi.Data.AdoNet;

// Task 7.1 - the connection string comes from configuration
// ("ConnectionStrings:StudentPortalSql" in appsettings.json, overridable by
// the ConnectionStrings__StudentPortalSql environment variable) and is never
// hard-coded in a repository. The repositories ask this factory for a NEW,
// unopened connection per operation. ADO.NET pools the underlying physical
// connections, so open/close per call is cheap and is the intended pattern.
public interface ISqlConnectionFactory
{
    SqlConnection Create();
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    public const string ConnectionName = "StudentPortalSql";

    private readonly string connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString(ConnectionName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionName}' is not configured.");
    }

    public SqlConnectionFactory(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public SqlConnection Create() => new(connectionString);
}
