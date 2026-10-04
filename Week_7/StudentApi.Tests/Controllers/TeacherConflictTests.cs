using System.Net;
using System.Net.Http.Json;
using StudentApi.Dtos;
using StudentApi.Tests.TestSupport;

namespace StudentApi.Tests.Controllers;

// Week 7 - TeachersController now maps the Service's Conflict outcome
// (duplicate email) to 409 on both POST and PUT.
public class TeacherConflictTests
{
    private static TeacherCreateDto Teacher(string email) => new() { Name = "Dr. Iyer", Email = email, Designation = "Professor" };

    [Fact]
    public async Task Create_DuplicateEmail_Returns409()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/teachers", Teacher("iyer@example.com"));

        var response = await client.PostAsJsonAsync("/api/teachers", Teacher("iyer@example.com"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ToAnotherTeachersEmail_Returns409()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/teachers", Teacher("first@example.com"));
        var second = await (await client.PostAsJsonAsync("/api/teachers", Teacher("second@example.com"))).Content.ReadFromJsonAsync<TeacherReadDto>();

        var response = await client.PutAsJsonAsync($"/api/teachers/{second!.Id}", Teacher("first@example.com"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_RollNumberLongerThanTheColumn_Returns400()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClientAs(StudentApi.Auth.Roles.Teacher);
        var tooLong = new StudentCreateDto { Name = "Asha", Age = 20, RollNumber = new string('R', 21), Email = "a@example.com", Score = 50 };

        var response = await client.PostAsJsonAsync("/api/students", tooLong);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
