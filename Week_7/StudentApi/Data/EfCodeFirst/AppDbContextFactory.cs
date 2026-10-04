using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Design;

namespace StudentApi.Data.EfCodeFirst;

// Used only by the `dotnet ef` tooling (migrations add / database update),
// so it can build an AppDbContext without starting the whole web app. It
// reads the same appsettings.json connection string the app uses.
[ExcludeFromCodeCoverage(Justification = "Design-time tooling entry point; never runs inside the app or tests.")]
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(configuration.GetConnectionString(DataLayerRegistration.EfCodeFirstConnection))
            .Options;

        return new AppDbContext(options);
    }
}
