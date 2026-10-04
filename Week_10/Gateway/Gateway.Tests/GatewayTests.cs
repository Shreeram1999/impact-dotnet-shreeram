using System.Net;
using Gateway.Tests.TestSupport;
using StudentPortal.Shared;

namespace Gateway.Tests;

// Task 10.5 / Testing Focus - the gateway validates the JWT centrally and
// forwards role claims; a tampered JWT is rejected AT THE GATEWAY.
public class GatewayTests
{
    // The required gateway-level test: flip one character of a valid
    // token's signature. 401 from the gateway, and the service never sees
    // the request at all.
    [Fact]
    public async Task TamperedJwt_IsRejectedAtTheGateway_AndNeverReachesAService()
    {
        await using var gateway = new GatewayHarness();
        var token = gateway.Token(Roles.Teacher);
        var tampered = token[..^3] + (token[^3] == 'A' ? 'B' : 'A') + token[^2..];

        var response = await gateway.Client(tampered).GetAsync("/academics/api/students");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(gateway.Stub.Requests);
    }

    [Fact]
    public async Task TamperedPayload_ClaimingTeacher_IsRejected()
    {
        await using var gateway = new GatewayHarness();
        var parts = gateway.Token(Roles.Student).Split('.');
        var forgedPayload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1]))).Replace("Student", "Teacher")))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var response = await gateway.Client($"{parts[0]}.{forgedPayload}.{parts[2]}").GetAsync("/academics/api/students");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(gateway.Stub.Requests);
    }

    [Theory]
    [InlineData("wrong-issuer")]
    [InlineData("wrong-key")]
    [InlineData("expired")]
    public async Task BadTokens_AreRejectedAtTheGateway(string kind)
    {
        await using var gateway = new GatewayHarness();
        var token = kind switch
        {
            "wrong-issuer" => gateway.Token(Roles.Teacher, issuer: "SomeoneElse"),
            "wrong-key" => gateway.Token(Roles.Teacher, key: new string('k', 48)),
            _ => gateway.Token(Roles.Teacher, expires: DateTime.UtcNow.AddMinutes(-10))
        };

        Assert.Equal(HttpStatusCode.Unauthorized, (await gateway.Client(token).GetAsync("/reporting/api/reports/terms")).StatusCode);
        Assert.Empty(gateway.Stub.Requests);
    }

    [Theory]
    [InlineData("/academics/api/students")]
    [InlineData("/reporting/api/reports/terms")]
    public async Task ProtectedRoutes_WithoutAToken_Are401(string url)
    {
        await using var gateway = new GatewayHarness();

        Assert.Equal(HttpStatusCode.Unauthorized, (await gateway.Client().GetAsync(url)).StatusCode);
        Assert.Empty(gateway.Stub.Requests);
    }

    [Fact]
    public async Task ValidToken_IsProxied_WithThePrefixStripped_AndRoleClaimsForwarded()
    {
        await using var gateway = new GatewayHarness();

        var response = await gateway.Client(gateway.Token(Roles.Teacher, "teacher1")).GetAsync("/academics/api/students?x=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var seen = Assert.Single(gateway.Stub.Requests);
        Assert.Equal(new SeenRequest("GET", "/api/students", "teacher1", Roles.Teacher, "42", HadAuthorization: true), seen);
    }

    [Fact]
    public async Task IdentityRoutes_AreAnonymous_SoLoginWorksLoggedOut()
    {
        await using var gateway = new GatewayHarness();

        var response = await gateway.Client().PostAsync("/identity/api/auth/login", new StringContent("{}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var seen = Assert.Single(gateway.Stub.Requests);
        Assert.Equal("/api/auth/login", seen.Path);
        Assert.Null(seen.Role);
    }

    // Nobody can become a Teacher by sending the header themselves: the
    // gateway drops client copies and writes the VALIDATED claims instead.
    [Fact]
    public async Task SpoofedRoleHeaders_AreReplacedWithTheTokensRealClaims()
    {
        await using var gateway = new GatewayHarness();
        var client = gateway.Client(gateway.Token(Roles.Student, "student1"));
        client.DefaultRequestHeaders.Add("X-User-Role", Roles.Teacher);
        client.DefaultRequestHeaders.Add("X-User-Name", "admin");

        await client.GetAsync("/academics/api/courses");

        var seen = Assert.Single(gateway.Stub.Requests);
        Assert.Equal(Roles.Student, seen.Role);
        Assert.Equal("student1", seen.UserName);
    }

    [Fact]
    public async Task SpoofedRoleHeaders_AreStripped_OnAnonymousRoutesToo()
    {
        await using var gateway = new GatewayHarness();
        var client = gateway.Client();
        client.DefaultRequestHeaders.Add("X-User-Role", Roles.Teacher);
        client.DefaultRequestHeaders.Add("X-User-Id", "1");

        await client.GetAsync("/identity/health");

        var seen = Assert.Single(gateway.Stub.Requests);
        Assert.Null(seen.Role);
        Assert.Null(seen.UserId);
    }

    [Fact]
    public async Task CorsPreflight_FromTheSpa_IsAnsweredAtTheEdge()
    {
        await using var gateway = new GatewayHarness();
        var request = new HttpRequestMessage(HttpMethod.Options, "/academics/api/students")
        {
            Headers =
            {
                { "Origin", "http://localhost:5173" },
                { "Access-Control-Request-Method", "GET" },
                { "Access-Control-Request-Headers", "authorization" }
            }
        };

        var response = await gateway.Client().SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Empty(gateway.Stub.Requests); // the preflight never needs a token or a service
    }

    [Fact]
    public async Task CorsPreflight_FromAnUnknownOrigin_GetsNoAllowOrigin()
    {
        await using var gateway = new GatewayHarness();
        var request = new HttpRequestMessage(HttpMethod.Options, "/academics/api/students")
        {
            Headers = { { "Origin", "http://evil.example" }, { "Access-Control-Request-Method", "GET" } }
        };

        var response = await gateway.Client().SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    // Task 10.10 - one service down: its routes fail cleanly (502), the
    // others keep working.
    [Fact]
    public async Task OneServiceDown_OnlyItsRoutesFail()
    {
        await using var gateway = new GatewayHarness(downService: "reporting");
        var client = gateway.Client(gateway.Token(Roles.Student));

        Assert.Equal(HttpStatusCode.BadGateway, (await client.GetAsync("/reporting/api/reports/terms")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/academics/api/students")).StatusCode);
    }

    [Fact]
    public async Task Health_IsServedByTheGatewayItself()
    {
        await using var gateway = new GatewayHarness();

        Assert.Contains("gateway", await gateway.Client().GetStringAsync("/health"));
        Assert.Empty(gateway.Stub.Requests);
    }

    private static string Pad(string base64Url)
    {
        var s = base64Url.Replace('-', '+').Replace('_', '/');
        return s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
    }
}
