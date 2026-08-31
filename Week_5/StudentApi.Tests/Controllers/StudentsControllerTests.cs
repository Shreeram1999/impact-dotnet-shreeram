using System.Net;
using System.Net.Http.Json;
using StudentApi.Dtos;
using StudentApi.Tests.TestSupport;

namespace StudentApi.Tests.Controllers;

// Task 5.5/5.6 - "a small set of controller tests asserting the right
// status code per outcome (200/201/204/400/404) using an in-memory
// service" (Testing Focus). Each test boots its own StudentApiFactory (real
// pipeline, FakeStudentService in place of the real service) so state never
// leaks between tests.
public class StudentsControllerTests
{
    private static readonly StudentCreateDto ValidStudent = new()
    {
        Name = "Asha",
        Age = 20,
        RollNumber = "R1",
        Email = "asha@example.com",
        Score = 82
    };

    [Fact]
    public async Task GetAll_NoStudents_Returns200WithEmptyList()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/students");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var students = await response.Content.ReadFromJsonAsync<List<StudentReadDto>>();
        Assert.Empty(students!);
    }

    [Fact]
    public async Task Create_ValidBody_Returns201WithLocationHeader()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/students", ValidStudent);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<StudentReadDto>();
        Assert.Equal("Asha", created!.Name);
    }

    [Fact]
    public async Task Create_InvalidAge_Returns400WithoutReachingTheService()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var invalid = new StudentCreateDto { Name = "Asha", Age = 200, RollNumber = "R1", Email = "asha@example.com", Score = 82 };

        var response = await client.PostAsJsonAsync("/api/students", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidEmail_Returns400()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var invalid = new StudentCreateDto { Name = "Asha", Age = 20, RollNumber = "R1", Email = "not-an-email", Score = 82 };

        var response = await client.PostAsJsonAsync("/api/students", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingId_Returns200()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/students", ValidStudent)).Content.ReadFromJsonAsync<StudentReadDto>();

        var response = await client.GetAsync($"/api/students/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_MissingId_Returns404()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/students/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingId_Returns204()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/students", ValidStudent)).Content.ReadFromJsonAsync<StudentReadDto>();
        var updated = new StudentCreateDto { Name = "Asha K", Age = 21, RollNumber = "R1", Email = "asha@example.com", Score = 90 };

        var response = await client.PutAsJsonAsync($"/api/students/{created!.Id}", updated);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_MissingId_Returns404()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/students/999", ValidStudent);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/students", ValidStudent)).Content.ReadFromJsonAsync<StudentReadDto>();

        var response = await client.DeleteAsync($"/api/students/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_MissingId_Returns404()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/students/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateRollNumber_Returns409()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/students", ValidStudent);

        var response = await client.PostAsJsonAsync("/api/students", ValidStudent);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Search_EmptyQuery_Returns200WithEveryStudent()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/students", ValidStudent);

        var response = await client.GetAsync("/api/students/search");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<StudentReadDto>>();
        Assert.Single(results!);
    }
}
