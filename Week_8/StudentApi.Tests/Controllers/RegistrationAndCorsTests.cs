using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using StudentApi.Auth;
using StudentApi.Dtos;
using StudentApi.Tests.TestSupport;

namespace StudentApi.Tests.Controllers;

// Task 8.5 - POST api/auth/register through the real pipeline (in-memory
// user store, real PBKDF2 hashing), then a real login with the new account.
public class RegistrationTests
{
    private static object Registration(string email = "new.student@example.com", string designation = "Student",
        string password = "Passw0rd", string dob = "2005-05-20") =>
        new { name = "New Student", dateOfBirth = dob, designation, email, password };

    [Fact]
    public async Task Register_Student_Returns201_AndTheAccountCanLogIn()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration());
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Username = "new.student@example.com", Password = "Passw0rd" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(new RegisterResponseDto("new.student@example.com", Roles.Student, "New Student"), await response.Content.ReadFromJsonAsync<RegisterResponseDto>());
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal(Roles.Student, (await login.Content.ReadFromJsonAsync<LoginResponseDto>())!.Role);
    }

    [Fact]
    public async Task Register_SameEmailTwice_Returns409()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", Registration());

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration(email: "NEW.STUDENT@example.com"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("weakpass")]   // no upper case / digit
    [InlineData("Sh0rt")]      // too short
    public async Task Register_WeakPassword_Returns400(string password)
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration(password: password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("designation", "Admin")]
    [InlineData("email", "not-an-email")]
    public async Task Register_InvalidField_Returns400(string field, string value)
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var body = field == "designation" ? Registration(designation: value) : Registration(email: value);

        var response = await client.PostAsJsonAsync("/api/auth/register", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_FutureDateOfBirth_Returns400WithAFieldError()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration(dob: "2099-01-01"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("DateOfBirth", await response.Content.ReadAsStringAsync());
    }

    // appsettings.Development.json enables Teacher self-registration for
    // local demos; the factory runs as Development.
    [Fact]
    public async Task Register_Teacher_InDevelopment_Returns201()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration(designation: "Teacher"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_Teacher_WhenDisabled_Returns403()
    {
        using var factory = new StudentApiFactory();
        using var configured = factory.WithWebHostBuilder(b => b.UseSetting("Registration:AllowTeacherSelfRegistration", "false"));
        using var client = configured.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration(designation: "Teacher"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}

// Week 8 - every write endpoint is Teacher-only now, not just POST students.
public class WriteEndpointAuthorizationTests
{
    private static readonly StudentCreateDto Student = new() { Name = "Asha", Age = 20, RollNumber = "R1", Email = "a@example.com", Score = 50 };
    private static readonly TeacherCreateDto Teacher = new() { Name = "Dr. Iyer", Email = "iyer@example.com", Designation = "Maths" };

    public static TheoryData<string, string> WriteRequests => new()
    {
        { "PUT", "/api/students/1" },
        { "DELETE", "/api/students/1" },
        { "POST", "/api/teachers" },
        { "PUT", "/api/teachers/1" },
        { "DELETE", "/api/teachers/1" }
    };

    private static HttpRequestMessage Build(string method, string url) => new(new HttpMethod(method), url)
    {
        Content = method == "DELETE" ? null : JsonContent.Create<object>(url.Contains("teachers") ? Teacher : Student)
    };

    [Theory]
    [MemberData(nameof(WriteRequests))]
    public async Task WithoutAToken_Returns401(string method, string url)
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Build(method, url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(WriteRequests))]
    public async Task WithAStudentToken_Returns403(string method, string url)
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClientAs(Roles.Student);

        var response = await client.SendAsync(Build(method, url));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}

// Task 8.9 - the CORS policy: the SPA origin is allowed, anything else gets
// no Access-Control-Allow-Origin header (so the browser blocks it).
public class CorsTests
{
    private static HttpRequestMessage Preflight(string origin, string method = "POST") => new(HttpMethod.Options, "/api/students")
    {
        Headers =
        {
            { "Origin", origin },
            { "Access-Control-Request-Method", method },
            { "Access-Control-Request-Headers", "authorization,content-type" }
        }
    };

    [Fact]
    public async Task Preflight_FromTheSpaOrigin_IsAllowed()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Preflight("http://localhost:5173"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Contains("POST", response.Headers.GetValues("Access-Control-Allow-Methods").Single());
    }

    [Fact]
    public async Task Preflight_FromAnUnknownOrigin_GetsNoAllowOriginHeader()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Preflight("http://evil.example"));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    // ASP.NET Core answers an allowed origin's preflight with the methods the
    // policy permits and lets the BROWSER enforce the comparison, so the
    // proof is that PATCH isn't in the list it advertises.
    [Fact]
    public async Task Preflight_ForAVerbOutsideThePolicy_DoesNotAdvertiseIt()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Preflight("http://localhost:5173", "PATCH"));

        var allowed = response.Headers.TryGetValues("Access-Control-Allow-Methods", out var values) ? string.Join(",", values) : "";
        Assert.DoesNotContain("PATCH", allowed);
    }

    [Fact]
    public async Task SimpleGet_FromTheSpaOrigin_CarriesTheAllowOriginHeader()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/students") { Headers = { { "Origin", "http://localhost:5173" } } };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }
}
