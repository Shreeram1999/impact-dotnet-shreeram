using Academics.Api.Data;
using Academics.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Academics.Tests.Data;

// EF Code First repositories against SQLite in-memory (real constraints,
// real cascades), each change checked through a second context.
public sealed class EfRepositoryTests : IDisposable
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly DbContextOptions<AcademicsDbContext> options;

    public EfRepositoryTests()
    {
        connection.Open();
        options = new DbContextOptionsBuilder<AcademicsDbContext>().UseSqlite(connection).Options;
        using var context = NewContext();
        context.Database.EnsureCreated();
    }

    public void Dispose() => connection.Dispose();

    private AcademicsDbContext NewContext() => new(options);

    [Fact]
    public void Seed_HasTeachersStudentsCoursesAndEnrollments()
    {
        using var context = NewContext();

        Assert.Equal(2, new EfRepository<Teacher>(context).GetAll().Count());
        Assert.Equal(3, new EfRepository<Student>(context).GetAll().Count());
        Assert.Equal(["MATH101", "PHYS101", "CS101"], new EfRepository<Course>(context).GetAll().Select(c => c.Code));
        Assert.Equal([1, 3], new EfEnrollmentRepository(context).StudentIdsFor(1));
    }

    [Fact]
    public void CourseCrud_RoundTrips()
    {
        var course = new Course { Code = "BIO101", Title = "Biology", Credits = 3, TeacherId = 2 };
        using (var context = NewContext())
        {
            var repository = new EfRepository<Course>(context);
            repository.Add(course);
            var tracked = repository.GetById(course.Id)!;
            tracked.Credits = 5;
            Assert.True(repository.Update(tracked));
        }

        using (var context = NewContext())
        {
            var repository = new EfRepository<Course>(context);
            Assert.Equal(5, repository.GetById(course.Id)!.Credits);
            Assert.True(repository.Update(new Course { Id = course.Id, Code = "BIO101", Title = "Bio", Credits = 2 })); // detached
            Assert.False(repository.Update(new Course { Id = 999, Code = "X", Title = "X", Credits = 1 }));
            Assert.True(repository.Delete(course.Id));
            Assert.False(repository.Delete(course.Id));
        }
    }

    [Fact]
    public void Enrollments_AddExistsRemove()
    {
        using var context = NewContext();
        var repository = new EfEnrollmentRepository(context);

        repository.Add(new Enrollment { StudentId = 2, CourseId = 3, EnrolledOn = new DateOnly(2026, 9, 1) });

        Assert.True(repository.Exists(2, 3));
        Assert.True(repository.Remove(2, 3));
        Assert.False(repository.Remove(2, 3));
        Assert.False(repository.Exists(2, 3));
    }

    [Fact]
    public void DeletingAStudent_CascadesToTheirEnrollments()
    {
        using (var context = NewContext())
            Assert.True(new EfRepository<Student>(context).Delete(1));

        using var verify = NewContext();
        Assert.DoesNotContain(verify.Enrollments, e => e.StudentId == 1);
    }

    [Fact]
    public void DeletingATeacher_LeavesTheirCoursesUnassigned()
    {
        using (var context = NewContext())
            Assert.True(new EfRepository<Teacher>(context).Delete(1));

        using var verify = NewContext();
        Assert.Null(verify.Courses.Single(c => c.Code == "MATH101").TeacherId);
    }

    [Fact]
    public void DuplicateCourseCode_IsRejectedByTheUniqueIndex()
    {
        using var context = NewContext();

        Assert.Throws<DbUpdateException>(() =>
            new EfRepository<Course>(context).Add(new Course { Code = "CS101", Title = "Again", Credits = 3 }));
    }

    [Fact]
    public void CreditsCheckConstraint_IsEnforced()
    {
        using var context = NewContext();

        Assert.Throws<DbUpdateException>(() =>
            new EfRepository<Course>(context).Add(new Course { Code = "ZZ1", Title = "Too heavy", Credits = 50 }));
    }
}
