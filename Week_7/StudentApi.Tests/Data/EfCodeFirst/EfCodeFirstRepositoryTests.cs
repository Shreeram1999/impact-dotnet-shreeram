using Microsoft.EntityFrameworkCore;
using StudentApi.Auth;
using StudentApi.Data.EfCodeFirst;
using StudentApi.Models;
using StudentApi.Services;

namespace StudentApi.Tests.Data.EfCodeFirst;

// EfStudentRepository / EfTeacherRepository / EfUserStore - CRUD round trips
// against SQLite in-memory, each verified through a SECOND context.
public class EfCodeFirstRepositoryTests : IDisposable
{
    private readonly SqliteAppDbContext database = new();

    public void Dispose() => database.Dispose();

    private static Student NewStudent(string roll = "R100", string email = "new@example.com") =>
        new() { Name = "New Student", Age = 22, RollNumber = roll, Email = email, Score = 70, EnrolledOn = new DateOnly(2026, 9, 1) };

    [Fact]
    public void GetAll_ReturnsTheHasDataSeed_InIdOrder()
    {
        using var context = database.CreateContext();

        var students = new EfStudentRepository(context).GetAll().ToList();

        Assert.Equal([1, 2, 3], students.Select(s => s.Id));
        Assert.Equal(new DateOnly(2026, 6, 15), students[2].EnrolledOn);
    }

    [Fact]
    public void Add_AssignsTheGeneratedId_AndPersists()
    {
        using (var context = database.CreateContext())
        {
            var student = NewStudent();
            new EfStudentRepository(context).Add(student);
            Assert.Equal(4, student.Id);
        }

        using var verify = database.CreateContext();
        Assert.Equal("R100", new EfStudentRepository(verify).GetById(4)!.RollNumber);
    }

    [Fact]
    public void Update_TrackedEntity_Persists()
    {
        using (var context = database.CreateContext())
        {
            var repository = new EfStudentRepository(context);
            var student = repository.GetById(2)!;
            student.Score = 99;
            Assert.True(repository.Update(student));
        }

        using var verify = database.CreateContext();
        Assert.Equal(99, verify.Students.Single(s => s.Id == 2).Score);
    }

    [Fact]
    public void Update_DetachedEntityWithAKnownId_Persists()
    {
        using (var context = database.CreateContext())
        {
            var detached = new Student { Id = 3, Name = "Divya N", Age = 19, RollNumber = "R003", Email = "divya.nair@school.example", Score = 95 };
            Assert.True(new EfStudentRepository(context).Update(detached));
        }

        using var verify = database.CreateContext();
        Assert.Equal("Divya N", verify.Students.Single(s => s.Id == 3).Name);
    }

    [Fact]
    public void Update_DetachedEntityWithAnUnknownId_ReturnsFalse()
    {
        using var context = database.CreateContext();
        var unknown = NewStudent();
        unknown.Id = 999;

        Assert.False(new EfStudentRepository(context).Update(unknown));
    }

    [Fact]
    public void Delete_ExistingAndMissing()
    {
        using (var context = database.CreateContext())
        {
            var repository = new EfStudentRepository(context);
            Assert.True(repository.Delete(2));
            Assert.False(repository.Delete(2));
        }

        using var verify = database.CreateContext();
        Assert.Null(new EfStudentRepository(verify).GetById(2));
    }

    // Task 7.6 - the unique Email index really exists in the generated schema.
    [Fact]
    public void UniqueEmailIndex_IsEnforcedByTheDatabase()
    {
        using var context = database.CreateContext();

        Assert.Throws<DbUpdateException>(() =>
            new EfStudentRepository(context).Add(NewStudent(roll: "R200", email: "asha.kumar@school.example")));
    }

    [Fact]
    public void AgeCheckConstraint_IsEnforcedByTheDatabase()
    {
        using var context = database.CreateContext();
        var student = NewStudent();
        student.Age = 150;

        Assert.Throws<DbUpdateException>(() => new EfStudentRepository(context).Add(student));
    }

    [Fact]
    public void TeacherRepository_SharesTheGenericImplementation()
    {
        using (var context = database.CreateContext())
        {
            var repository = new EfTeacherRepository(context);
            Assert.Equal(2, repository.GetAll().Count());
            repository.Add(new Teacher { Name = "New Teacher", Email = "nt@example.com", Designation = "Chemistry" });
        }

        using var verify = database.CreateContext();
        Assert.Equal(3, new EfTeacherRepository(verify).GetAll().Count());
    }

    [Fact]
    public void Service_OverTheRealEfRepository_EnforcesConflicts_AndPersists()
    {
        using var context = database.CreateContext();
        var service = new StudentService(new EfStudentRepository(context));

        Assert.Equal(OperationOutcome.Conflict, service.Add(NewStudent(roll: "R001")).Outcome);
        Assert.Equal(OperationOutcome.Success, service.Add(NewStudent()).Outcome);
        Assert.Equal(OperationOutcome.Success, service.Delete(1).Outcome);
        Assert.Equal(OperationOutcome.NotFound, service.Delete(1).Outcome);
    }

    [Fact]
    public void UserStore_FindsSeededUsersCaseInsensitively()
    {
        using var context = database.CreateContext();
        var store = new EfUserStore(context);

        var teacher = store.FindByUsername("TEACHER1");

        Assert.Equal(Roles.Teacher, teacher!.Role);
        Assert.Equal(SeedData.TeacherPasswordHash, teacher.PasswordHash);
        Assert.Null(store.FindByUsername("nobody"));
    }

    [Fact]
    public void UserStore_SeededHash_VerifiesWithTheRealHasher()
    {
        using var context = database.CreateContext();

        var student = new EfUserStore(context).FindByUsername("student1")!;

        Assert.True(new Pbkdf2PasswordHasher().Verify("Student@123", student.PasswordHash));
    }

    [Fact]
    public void UserStore_Add_ThenDuplicateThrows()
    {
        using var context = database.CreateContext();
        var store = new EfUserStore(context);

        store.Add(new User { Username = "teacher2", PasswordHash = "h", Role = Roles.Teacher });

        Assert.NotNull(store.FindByUsername("teacher2"));
        Assert.Throws<InvalidOperationException>(() => store.Add(new User { Username = "Teacher2", PasswordHash = "h", Role = Roles.Teacher }));
    }
}
