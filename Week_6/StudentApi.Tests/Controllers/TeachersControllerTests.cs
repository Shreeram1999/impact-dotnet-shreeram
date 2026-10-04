using System.Net;
using System.Net.Http.Json;
using StudentApi.Dtos;
using StudentApi.Tests.TestSupport;

namespace StudentApi.Tests.Controllers;

// Task 5.10 (stretch) - the same status-code matrix StudentsControllerTests
// covers, mirrored for TeachersController.
public class TeachersControllerTests
{
    private static readonly TeacherCreateDto ValidTeacher = new()
    {
        Name = "Dr. Iyer",
        Email = "iyer@example.com",
        Designation = "Professor"
    };

    [Fact]
    public async Task Create_ValidBody_Returns201WithLocationHeader()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/teachers", ValidTeacher);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Create_InvalidEmail_Returns400()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var invalid = new TeacherCreateDto { Name = "Dr. Iyer", Email = "not-an-email", Designation = "Professor" };

        var response = await client.PostAsJsonAsync("/api/teachers", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_MissingId_Returns404()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/teachers/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingId_Returns204()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/teachers", ValidTeacher)).Content.ReadFromJsonAsync<TeacherReadDto>();
        var updated = new TeacherCreateDto { Name = "Dr. Iyer", Email = "iyer@example.com", Designation = "HOD" };

        var response = await client.PutAsJsonAsync($"/api/teachers/{created!.Id}", updated);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingId_Returns204()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/teachers", ValidTeacher)).Content.ReadFromJsonAsync<TeacherReadDto>();

        var response = await client.DeleteAsync($"/api/teachers/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
