using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StudentApi.Auth;
using StudentApi.Models;

namespace StudentApi.Tests.TestSupport;

// Task 6.15 - POST api/students is now Teacher-only, so the Week 5
// controller tests that create students need a client carrying a real,
// signed Teacher token. The token comes from the app's own ITokenService,
// so it's validated by the real JwtBearer middleware exactly like a token
// from POST api/auth/login.
public static class AuthenticatedClientExtensions
{
    public static HttpClient CreateClientAs<TEntryPoint>(this WebApplicationFactory<TEntryPoint> factory, string role)
        where TEntryPoint : class
    {
        var tokens = factory.Services.GetRequiredService<ITokenService>();
        var token = tokens.CreateToken(new User { Id = 99, Username = $"{role.ToLowerInvariant()}-test", Role = role });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }
}
