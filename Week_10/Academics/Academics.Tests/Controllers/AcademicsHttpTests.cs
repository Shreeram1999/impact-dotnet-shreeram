using System.Net;
using System.Net.Http.Json;
using Academics.Api.Dtos;
using Academics.Tests.TestSupport;
using StudentPortal.Shared;

namespace Academics.Tests.Controllers;

// Task 10.3 "CRUD works and is role-protected", end to end through the real
// pipeline on SQLite: every status code per role, for all three resources.
public class AcademicsHttpTests
{
    private static readonly StudentCreateDto NewStudent = new() { Name = "Kiran Das", Age = 22, RollNumber = "R010", Email = "kiran@school.example", Score = 77 };
    private static readonly TeacherCreateDto NewTeacher = new() { Name = "Dr. Sen", Email = "sen@school.example", Designation = "Chemistry" };
    private static readonly CourseCreateDto NewCourse = new() { Code = "chem101", Title = "Chemistry I", Credits = 3, TeacherId = 2 };

    [Theory]
    [InlineData("/api/students")]
    [InlineData("/api/teachers")]
    [InlineData("/api/courses")]
    public async Task Reads_NeedAToken_EvenBehindTheGateway(string url)
    {
        using var factory = new AcademicsApiFactory();

        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClientAs(null).GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await factory.CreateClientAs(Roles.Student).GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Student_CannotWriteAnything()
    {
        using var factory = new AcademicsApiFactory();
        var client = factory.CreateClientAs(Roles.Student);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/students", NewStudent)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/teachers", NewTeacher)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/courses", NewCourse)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/courses/1/enrollments", new EnrollmentCreateDto { StudentId = 2 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync("/api/students/1")).StatusCode);
    }

    [Fact]
    public async Task Teacher_StudentCrud()
    {
        using var factory = new AcademicsApiFactory();
        var client = factory.CreateClientAs(Roles.Teacher);

        var created = await client.PostAsJsonAsync("/api/students", NewStudent);
        var id = (await created.Content.ReadFromJsonAsync<StudentReadDto>())!.Id;

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/students", NewStudent)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"/api/students/{id}", NewStudent)).StatusCode);
        Assert.Equal("77%", (await client.GetFromJsonAsync<GradeDto>($"/api/students/{id}/grade"))!.Grade);
        Assert.Single(await client.GetFromJsonAsync<List<StudentReadDto>>("/api/students/search?name=kiran") ?? []);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/students/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/students/{id}")).StatusCode);
    }

    [Fact]
    public async Task Teacher_TeacherCrud()
    {
        using var factory = new AcademicsApiFactory();
        var client = factory.CreateClientAs(Roles.Teacher);

        var created = await client.PostAsJsonAsync("/api/teachers", NewTeacher);
        var id = (await created.Content.ReadFromJsonAsync<TeacherReadDto>())!.Id;

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/teachers", NewTeacher)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"/api/teachers/{id}", NewTeacher)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/teachers/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/teachers/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/teachers/{id}")).StatusCode);
    }

    [Fact]
    public async Task Teacher_CourseCrud_AndEnrollments()
    {
        using var factory = new AcademicsApiFactory();
        var client = factory.CreateClientAs(Roles.Teacher);

        var created = await client.PostAsJsonAsync("/api/courses", NewCourse);
        var course = (await created.Content.ReadFromJsonAsync<CourseReadDto>())!;
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("CHEM101", course.Code); // normalised by the mapper

        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/courses", NewCourse)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/courses", new CourseCreateDto { Code = "X9", Title = "Ghost", Credits = 2, TeacherId = 99 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/courses", new CourseCreateDto { Code = "X9", Title = "Too many", Credits = 11 })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"/api/courses/{course.Id}", NewCourse)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/courses/999", NewCourse)).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync($"/api/courses/{course.Id}/enrollments", new EnrollmentCreateDto { StudentId = 2 })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync($"/api/courses/{course.Id}/enrollments", new EnrollmentCreateDto { StudentId = 2 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync($"/api/courses/{course.Id}/enrollments", new EnrollmentCreateDto { StudentId = 404 })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/courses/999/enrollments", new EnrollmentCreateDto { StudentId = 2 })).StatusCode);

        var roster = await client.GetFromJsonAsync<List<StudentReadDto>>($"/api/courses/{course.Id}/students");
        Assert.Equal("Rohit Menon", Assert.Single(roster!).Name);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/courses/999/students")).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/courses/{course.Id}/enrollments/2")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/courses/{course.Id}/enrollments/2")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/courses/{course.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/courses/{course.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/courses/{course.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/courses/{course.Id}")).StatusCode);
    }

    [Fact]
    public async Task InvalidStudent_Is400_AndHealthIsAnonymous()
    {
        using var factory = new AcademicsApiFactory();
        var client = factory.CreateClientAs(Roles.Teacher);

        var invalid = new StudentCreateDto { Name = "", Age = 3, RollNumber = "R", Email = "nope", Score = 0 };

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/students", invalid)).StatusCode);
        Assert.Contains("academics", await factory.CreateClientAs(null).GetStringAsync("/health"));
    }
}
