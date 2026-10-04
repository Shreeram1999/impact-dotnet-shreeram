using System.Net.Http.Headers;
using System.Security.Claims;
using Academics.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StudentPortal.Shared;

namespace Academics.Tests.TestSupport;

// The real Academics pipeline (controllers, services, EF repositories, JWT
// validation) on a SQLite in-memory database built from the same model and
// HasData seed - so HTTP tests exercise real SQL without SQL Server.
public sealed class AcademicsApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");

    public AcademicsApiFactory()
    {
        connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // AddDbContext registers both the options AND an options-
            // configuration callback (UseSqlServer); both must go, or EF sees
            // two providers.
            services.RemoveAll<DbContextOptions<AcademicsDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AcademicsDbContext>>();
            services.AddDbContext<AcademicsDbContext>(options => options.UseSqlite(connection));
        });
    }

    public void EnsureDatabase()
    {
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AcademicsDbContext>().Database.EnsureCreated();
    }

    // A token signed exactly the way Identity signs (same key/issuer/
    // audience from configuration, same claim names) - this service can't
    // tell it apart from a real login.
    public HttpClient CreateClientAs(string? role)
    {
        EnsureDatabase();
        var client = CreateClient();
        if (role is null)
            return client;

        var jwt = Services.GetRequiredService<IOptions<JwtOptions>>().Value;
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = DateTime.UtcNow.AddMinutes(10),
            Subject = new ClaimsIdentity([new Claim(PortalClaims.Name, $"{role.ToLower()}-test"), new Claim(PortalClaims.Role, role)]),
            SigningCredentials = new SigningCredentials(jwt.SigningKey(), SecurityAlgorithms.HmacSha256)
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            connection.Dispose();
    }
}

internal static class ServiceCollectionExtensions
{
    public static void RemoveAll<T>(this IServiceCollection services)
    {
        foreach (var descriptor in services.Where(d => d.ServiceType == typeof(T)).ToList())
            services.Remove(descriptor);
    }
}
