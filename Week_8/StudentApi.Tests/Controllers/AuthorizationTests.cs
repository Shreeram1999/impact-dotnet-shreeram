using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StudentApi.Auth;
using StudentApi.Dtos;
using StudentApi.Tests.TestSupport;

namespace StudentApi.Tests.Controllers;

// Task 6.15 end to end through the real pipeline: real login, real PBKDF2
// verification against the seeded users, real JwtBearer validation, and the
// role check on POST api/students.
public class AuthorizationTests
{
    private static readonly StudentCreateDto ValidStudent = new()
    {
        Name = "Asha",
        Age = 20,
        RollNumber = "R1",
        Email = "asha@example.com",
        Score = 82
    };

    private static async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Username = username, Password = password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponseDto>())!.Token;
    }

    [Fact]
    public async Task Login_SeededTeacher_Returns200WithTokenAndRole()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Username = "teacher1", Password = "Teacher@123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.Equal(Roles.Teacher, body!.Role);
        Assert.Equal(3, body.Token.Split('.').Length);
    }

    [Theory]
    [InlineData("teacher1", "wrong-password")]
    [InlineData("nobody", "Teacher@123")]
    public async Task Login_BadCredentials_Returns401(string username, string password)
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Username = username, Password = password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_MissingFields_Returns400()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "teacher1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithoutAToken_Returns401()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/students", ValidStudent);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithAStudentToken_Returns403()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "student1", "Student@123"));

        var response = await client.PostAsJsonAsync("/api/students", ValidStudent);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithATeacherToken_Returns201()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "teacher1", "Teacher@123"));

        var response = await client.PostAsJsonAsync("/api/students", ValidStudent);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateStudent_WithATamperedToken_Returns401()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var token = await LoginAsync(client, "teacher1", "Teacher@123");
        var tampered = token[..^2] + (token[^2] == 'A' ? "B" : "A") + token[^1];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tampered);

        var response = await client.PostAsJsonAsync("/api/students", ValidStudent);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReadEndpoints_StayAnonymous()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/students");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithAToken_ReturnsTheUsernameAndRole()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(client, "student1", "Student@123"));

        var me = await client.GetFromJsonAsync<CurrentUserDto>("/api/auth/me");

        Assert.Equal(new CurrentUserDto("student1", Roles.Student, "Asha Kumar"), me);
    }

    [Fact]
    public async Task Me_WithoutAToken_Returns401()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_DocumentsTheBearerScheme()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var json = await client.GetStringAsync("/swagger/v1/swagger.json");

        Assert.Contains("\"bearer\"", json);
        Assert.Contains("/api/auth/login", json);
    }
}
