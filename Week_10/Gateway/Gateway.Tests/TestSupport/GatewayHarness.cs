using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StudentPortal.Shared;

namespace Gateway.Tests.TestSupport;

public record SeenRequest(string Method, string Path, string? UserName, string? Role, string? UserId, bool HadAuthorization);

// A real HTTP service on a random local port standing in for Identity,
// Academics and Reporting: it records exactly what the gateway forwarded.
public sealed class StubService : IAsyncDisposable
{
    private readonly WebApplication app;

    public ConcurrentQueue<SeenRequest> Requests { get; } = new();
    public string Url { get; }

    public StubService()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        app = builder.Build();
        app.Run(async context =>
        {
            var headers = context.Request.Headers;
            Requests.Enqueue(new SeenRequest(context.Request.Method, context.Request.Path.Value ?? "",
                headers["X-User-Name"].FirstOrDefault(), headers["X-User-Role"].FirstOrDefault(),
                headers["X-User-Id"].FirstOrDefault(), headers.ContainsKey("Authorization")));
            await context.Response.WriteAsJsonAsync(new { reached = context.Request.Path.Value });
        });
        app.StartAsync().GetAwaiter().GetResult();
        Url = app.Urls.First();
    }

    public async ValueTask DisposeAsync() => await app.DisposeAsync();
}

// The real gateway (Program.cs + appsettings) with every cluster pointed at
// the stub (or, for outage tests, at a port nothing listens on).
public sealed class GatewayHarness : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> factory;

    public StubService Stub { get; } = new();

    public GatewayHarness(string? downService = null)
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            foreach (var cluster in new[] { "identity", "academics", "reporting" })
            {
                var address = cluster == downService ? "http://127.0.0.1:1/" : Stub.Url + "/";
                builder.UseSetting($"ReverseProxy:Clusters:{cluster}:Destinations:d1:Address", address);
            }
        });
    }

    public HttpClient Client(string? token = null)
    {
        var client = factory.CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // Signed the way Identity signs. Overrides let tests forge bad tokens.
    public string Token(string role, string username = "user1", string? issuer = null, string? key = null, DateTime? expires = null)
    {
        var jwt = factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
        var signingKey = key is null ? jwt.SigningKey() : new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
        var expiry = expires ?? DateTime.UtcNow.AddMinutes(10);
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer ?? jwt.Issuer,
            Audience = jwt.Audience,
            NotBefore = expiry.AddHours(-1),
            IssuedAt = expiry.AddHours(-1),
            Expires = expiry,
            Subject = new ClaimsIdentity(
            [
                new Claim(PortalClaims.Name, username),
                new Claim(PortalClaims.Role, role),
                new Claim(PortalClaims.UserId, "42")
            ]),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        });
    }

    public async ValueTask DisposeAsync()
    {
        factory.Dispose();
        await Stub.DisposeAsync();
    }
}
