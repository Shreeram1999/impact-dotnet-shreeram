using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StudentApi.Auth;
using StudentApi.Data;
using StudentApi.Dtos;
using StudentApi.Models;
using StudentApi.Services;
using StudentApi.Tests.TestSupport;

namespace StudentApi.Tests.Canonical;

// Task 9.3 - the five canonical service tests, gathered in one place. Every
// one runs with the REPOSITORY MOCKED (Moq), so none of them touches a
// database. Where the outcome is an HTTP status, the real StudentService
// runs inside the real pipeline on top of the mocked repository.
public class CanonicalServiceTests
{
    private static readonly StudentCreateDto ValidDto = new() { Name = "Asha", Age = 20, RollNumber = "R1", Email = "asha@example.com", Score = 82 };

    private static (WebApplicationFactory<Program> App, Mock<IRepository<Student>> Repository) PortalWithMockedRepository()
    {
        var repository = new Mock<IRepository<Student>>();
        repository.Setup(r => r.GetAll()).Returns([]);
        var app = new PortalApiFactory().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped(_ => repository.Object)));
        return (app, repository);
    }

    // 1. Valid create
    [Fact]
    public void ValidCreate_Succeeds_AndReachesTheRepositoryOnce()
    {
        var repository = new Mock<IRepository<Student>>();
        repository.Setup(r => r.GetAll()).Returns([]);
        repository.Setup(r => r.Add(It.IsAny<Student>())).Callback<Student>(s => s.Id = 42);

        var result = new StudentService(repository.Object).Add(StudentMapper.ToEntity(ValidDto));

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        Assert.Equal(42, result.Value!.Id);
        repository.Verify(r => r.Add(It.IsAny<Student>()), Times.Once);
    }

    // 2. Missing id -> 404 path
    [Fact]
    public async Task MissingId_IsNotFound_InTheServiceAnd404OverHttp()
    {
        var repository = new Mock<IRepository<Student>>();
        repository.Setup(r => r.GetById(999)).Returns((Student?)null);
        Assert.Equal(OperationOutcome.NotFound, new StudentService(repository.Object).Delete(999).Outcome);
        repository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);

        var (app, httpRepository) = PortalWithMockedRepository();
        using (app)
        {
            using var client = app.CreateClientAs(Roles.Teacher);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/students/999")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/students/999", ValidDto)).StatusCode);
            httpRepository.Verify(r => r.Update(It.IsAny<Student>()), Times.Never);
        }
    }

    // 3. Invalid input -> 400 path
    [Fact]
    public async Task InvalidInput_Is400_AndNeverReachesTheServiceOrRepository()
    {
        var (app, repository) = PortalWithMockedRepository();
        using (app)
        {
            using var client = app.CreateClientAs(Roles.Teacher);
            var invalid = new StudentCreateDto { Name = "", Age = 3, RollNumber = "R1", Email = "nope", Score = 101 };

            var response = await client.PostAsJsonAsync("/api/students", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Contains("Age", problem!.Errors.Keys);
            Assert.Contains("Email", problem.Errors.Keys);
            repository.Verify(r => r.Add(It.IsAny<Student>()), Times.Never);
        }
    }

    // 4. Simulated repository error is handled
    [Fact]
    public async Task SimulatedRepositoryError_BecomesAClean500ProblemDetails_WithNoInternalsLeaked()
    {
        var (app, repository) = PortalWithMockedRepository();
        repository.Setup(r => r.Add(It.IsAny<Student>()))
            .Throws(new InvalidOperationException("Connection to SQL Server 'db01' failed: secret detail"));
        using (app)
        {
            using var client = app.CreateClientAs(Roles.Teacher);

            var response = await client.PostAsJsonAsync("/api/students", ValidDto);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("An unexpected error occurred.", body);
            Assert.Contains("traceId", body);
            Assert.DoesNotContain("secret detail", body);
            Assert.DoesNotContain("StudentService", body); // no stack trace
        }
    }

    // ...and the Service itself doesn't swallow it (no fake "success").
    [Fact]
    public void SimulatedRepositoryError_IsNotSwallowedByTheService()
    {
        var repository = new Mock<IRepository<Student>>();
        repository.Setup(r => r.GetAll()).Throws(new TimeoutException("db timeout"));

        Assert.Throws<TimeoutException>(() => new StudentService(repository.Object).Add(StudentMapper.ToEntity(ValidDto)));
    }

    // 5. Student blocked on write / Teacher allowed
    [Theory]
    [InlineData(Roles.Student, HttpStatusCode.Forbidden, 0)]
    [InlineData(Roles.Teacher, HttpStatusCode.Created, 1)]
    public async Task Write_IsBlockedForStudents_AndAllowedForTeachers(string role, HttpStatusCode expected, int expectedAdds)
    {
        var (app, repository) = PortalWithMockedRepository();
        using (app)
        {
            using var client = app.CreateClientAs(role);

            var response = await client.PostAsJsonAsync("/api/students", ValidDto);

            Assert.Equal(expected, response.StatusCode);
            repository.Verify(r => r.Add(It.IsAny<Student>()), Times.Exactly(expectedAdds));
        }
    }
}
