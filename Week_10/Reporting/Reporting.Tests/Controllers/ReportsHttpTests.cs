using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Reporting.Api.Data;
using Reporting.Api.Services;
using Reporting.Tests.TestSupport;
using StudentPortal.Shared;

namespace Reporting.Tests.Controllers;

// The Reporting HTTP surface on the SQLite copy: auth required, both roles
// may read, 404 for an unknown term.
public sealed class ReportsHttpTests : IDisposable
{
    private readonly ReportingTestDatabase database = new();
    private readonly WebApplicationFactory<Program> factory;

    public ReportsHttpTests()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            foreach (var d in services.Where(d => d.ServiceType == typeof(DbContextOptions<ReportingDbContext>)
                                                  || d.ServiceType == typeof(IDbContextOptionsConfiguration<ReportingDbContext>)).ToList())
                services.Remove(d);
            services.AddDbContext<ReportingDbContext>(o => o.UseSqlite(database.Connection));
        }));
    }

    public void Dispose()
    {
        factory.Dispose();
        database.Dispose();
    }

    private HttpClient Client(string? role)
    {
        var client = factory.CreateClient();
        if (role is null)
            return client;

        var jwt = factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = DateTime.UtcNow.AddMinutes(5),
            Subject = new ClaimsIdentity([new Claim(PortalClaims.Name, "u"), new Claim(PortalClaims.Role, role)]),
            SigningCredentials = new SigningCredentials(jwt.SigningKey(), SecurityAlgorithms.HmacSha256)
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Theory]
    [InlineData("/api/reports/terms")]
    [InlineData("/api/reports/enrollment-summary")]
    [InlineData("/api/reports/departments")]
    public async Task EveryReport_RequiresAToken(string url)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await Client(null).GetAsync(url)).StatusCode);
    }

    [Theory]
    [InlineData(Roles.Student)]
    [InlineData(Roles.Teacher)]
    public async Task BothRoles_CanReadTheAggregates(string role)
    {
        var client = Client(role);

        var terms = await client.GetFromJsonAsync<List<TermDto>>("/api/reports/terms");
        var summary = await client.GetFromJsonAsync<EnrollmentSummaryDto>("/api/reports/enrollment-summary?termId=1");
        var departments = await client.GetFromJsonAsync<List<DepartmentSummaryDto>>("/api/reports/departments?termId=1");

        Assert.Equal(2, terms!.Count);
        Assert.Equal(11, summary!.TotalEnrolled);
        Assert.Equal(3, departments!.Count);
    }

    [Theory]
    [InlineData("/api/reports/enrollment-summary?termId=42")]
    [InlineData("/api/reports/departments?termId=42")]
    public async Task UnknownTerm_Is404(string url)
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Client(Roles.Student).GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Health_IsAnonymous()
    {
        Assert.Contains("reporting", await Client(null).GetStringAsync("/health"));
    }
}
