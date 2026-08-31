using System.Net;
using System.Net.Http.Json;
using StudentApi.Dtos;
using StudentApi.Tests.TestSupport;

namespace StudentApi.Tests.Controllers;

// Task 5.8 - GET api/students/{id}/grade end to end: both scales, and the
// 404 path, through the real pipeline.
public class StudentGradeEndpointTests
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
    public async Task GetGrade_DefaultScale_ReturnsPercentage()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/students", ValidStudent)).Content.ReadFromJsonAsync<StudentReadDto>();

        var response = await client.GetAsync($"/api/students/{created!.Id}/grade");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var grade = await response.Content.ReadFromJsonAsync<GradeDto>();
        Assert.Equal("82%", grade!.Grade);
    }

    [Fact]
    public async Task GetGrade_GpaScale_ReturnsGpa()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/students", ValidStudent)).Content.ReadFromJsonAsync<StudentReadDto>();

        var response = await client.GetAsync($"/api/students/{created!.Id}/grade?scale=gpa");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var grade = await response.Content.ReadFromJsonAsync<GradeDto>();
        Assert.Equal("3.28 GPA", grade!.Grade);
    }

    [Fact]
    public async Task GetGrade_MissingStudent_Returns404()
    {
        using var factory = new StudentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/students/999/grade");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
