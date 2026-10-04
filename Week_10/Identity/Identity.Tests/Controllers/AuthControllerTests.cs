using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Identity.Api.Data;
using Identity.Api.Dtos;
using Identity.Tests.TestSupport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StudentPortal.Shared;

namespace Identity.Tests.Controllers;

// The Identity HTTP surface end to end (real pipeline, real PBKDF2, real
// JWT issuing + validation), with only the SQL store swapped for memory.
public class AuthControllerTests
{
    private static object Registration(string email = "new@school.example", string designation = "Student", string dob = "2005-05-20") =>
        new { name = "New Person", dateOfBirth = dob, designation, email, password = "Passw0rd" };

    private static async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Username = username, Password = password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponseDto>())!.Token;
    }

    [Fact]
    public async Task Login_SeededTeacher_Returns200WithAThreePartJwt()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Username = "teacher1", Password = "Teacher@123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.Equal(Roles.Teacher, body!.Role);
        Assert.Equal(3, body.Token.Split('.').Length);
    }

    [Theory]
    [InlineData("teacher1", "wrong")]
    [InlineData("ghost", "Teacher@123")]
    public async Task Login_BadCredentials_Return401(string username, string password)
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Username = username, Password = password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_ThenLogin_Works_AndMeReflectsTheToken()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();

        var registered = await client.PostAsJsonAsync("/api/auth/register", Registration());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "new@school.example", "Passw0rd"));
        var me = await client.GetFromJsonAsync<CurrentUserDto>("/api/auth/me");

        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        Assert.Equal(new CurrentUserDto("new@school.example", Roles.Student, "New Person"), me);
    }

    [Fact]
    public async Task Register_Duplicate_Returns409()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", Registration());

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_FutureDateOfBirth_Returns400()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration(dob: "2099-01-01"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_Teacher_WhenDisabled_Returns403()
    {
        using var factory = new IdentityApiFactory();
        using var configured = factory.WithWebHostBuilder(b => b.UseSetting("Registration:AllowTeacherSelfRegistration", "false"));
        using var client = configured.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration(designation: "Teacher"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutAToken_Returns401()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    // IdentityDb unreachable: the shared ApiExceptionHandler answers with a
    // clean ProblemDetails 500 - no SQL error text leaks to the caller.
    [Fact]
    public async Task DatabaseOutage_DuringLogin_IsAClean500()
    {
        var failing = new Mock<IUserStore>();
        failing.Setup(s => s.FindByUsername(It.IsAny<string>())).Throws(new InvalidOperationException("SQL timeout on db-host-7"));
        using var factory = new IdentityApiFactory();
        using var configured = factory.WithWebHostBuilder(b => b.ConfigureTestServices(services => services.AddSingleton(failing.Object)));
        using var client = configured.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Username = "teacher1", Password = "x" });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("An unexpected error occurred.", body);
        Assert.DoesNotContain("db-host-7", body);
    }

    [Fact]
    public async Task Health_IsAnonymous()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();

        Assert.Contains("identity", await client.GetStringAsync("/health"));
    }
}
