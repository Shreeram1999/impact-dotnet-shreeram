using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StudentApi.Data.EfCodeFirst;

namespace StudentApi.Tests.Data.EfCodeFirst;

// Testing Focus - "integration tests using the EF Core ... SQLite-in-memory
// provider". A real relational engine (unlike EF's InMemory provider it
// enforces unique indexes, check constraints and foreign keys), living only
// as long as this one open connection - so each test gets a fresh, seeded
// database in milliseconds and nothing touches SQL Server.
public sealed class SqliteAppDbContext : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly DbContextOptions<AppDbContext> options;

    public SqliteAppDbContext()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

        using var context = CreateContext();
        context.Database.EnsureCreated(); // builds the schema from the model + HasData seed
    }

    // A NEW context over the SAME database - used to prove a change was
    // really saved, not just held in the first context's change tracker.
    public AppDbContext CreateContext() => new(options);

    public void Dispose() => connection.Dispose();
}
