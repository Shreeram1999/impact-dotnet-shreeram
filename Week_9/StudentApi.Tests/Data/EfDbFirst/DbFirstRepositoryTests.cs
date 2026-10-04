using Microsoft.EntityFrameworkCore;
using StudentApi.Auth;
using StudentApi.Data.EfDbFirst;
using StudentApi.Models;

namespace StudentApi.Tests.Data.EfDbFirst;

// The DB First repositories' mapping logic (scaffolded entity <-> domain
// model), over EF's InMemory provider. The scaffolded context's SQL Server
// details (filtered indexes, named defaults) are ignored by InMemory, which
// is fine here: what's under test is the repository code, while the schema
// itself is SQL Server's job and is exercised by smoke-tests.ps1.
public class DbFirstRepositoryTests
{
    private readonly DbContextOptions<StudentPortalDbFirstContext> options =
        new DbContextOptionsBuilder<StudentPortalDbFirstContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    private StudentPortalDbFirstContext NewContext() => new(options);

    private static Student NewStudent() =>
        new() { Name = "Asha", Age = 20, RollNumber = "R1", Email = "asha@example.com", Score = 82, InternalNotes = "note", EnrolledOn = new DateOnly(2026, 6, 1), UserId = 5 };

    [Fact]
    public void Student_AddThenRead_MapsEveryField()
    {
        var student = NewStudent();
        using (var context = NewContext())
            new DbFirstStudentRepository(context).Add(student);

        using var verify = NewContext();
        var loaded = new DbFirstStudentRepository(verify).GetById(student.Id)!;
        Assert.True(student.Id > 0);
        Assert.Equivalent(student, loaded);
        Assert.Single(new DbFirstStudentRepository(verify).GetAll());
    }

    [Fact]
    public void Student_Update_CopiesFieldsOntoTheScaffoldedRow()
    {
        var student = NewStudent();
        using (var context = NewContext())
            new DbFirstStudentRepository(context).Add(student);

        using (var context = NewContext())
        {
            student.Name = "Asha Kumar";
            student.EnrolledOn = null;
            Assert.True(new DbFirstStudentRepository(context).Update(student));
        }

        using var verify = NewContext();
        var row = verify.Students.Single();
        Assert.Equal("Asha Kumar", row.Name);
        Assert.Null(row.EnrolledOn);
    }

    [Fact]
    public void Student_UpdateOrDeleteUnknownId_ReturnsFalse_AndGetByIdReturnsNull()
    {
        using var context = NewContext();
        var repository = new DbFirstStudentRepository(context);

        Assert.False(repository.Update(new Student { Id = 42 }));
        Assert.False(repository.Delete(42));
        Assert.Null(repository.GetById(42));
    }

    [Fact]
    public void Student_Delete_RemovesTheRow()
    {
        var student = NewStudent();
        using (var context = NewContext())
            new DbFirstStudentRepository(context).Add(student);

        using (var context = NewContext())
            Assert.True(new DbFirstStudentRepository(context).Delete(student.Id));

        using var verify = NewContext();
        Assert.Empty(verify.Students);
    }

    [Fact]
    public void Teacher_FullCrud()
    {
        var teacher = new Teacher { Name = "Dr. Iyer", Email = "iyer@example.com", Designation = "Maths" };
        using (var context = NewContext())
        {
            var repository = new DbFirstTeacherRepository(context);
            repository.Add(teacher);
            teacher.Designation = "Head of Maths";
            Assert.True(repository.Update(teacher));
        }

        using (var context = NewContext())
        {
            var repository = new DbFirstTeacherRepository(context);
            Assert.Equal("Head of Maths", repository.GetById(teacher.Id)!.Designation);
            Assert.Single(repository.GetAll());
            Assert.True(repository.Delete(teacher.Id));
            Assert.False(repository.Delete(teacher.Id));
            Assert.False(repository.Update(teacher));
            Assert.Null(repository.GetById(teacher.Id));
        }
    }

    [Fact]
    public void UserStore_AddFindAndRejectDuplicates()
    {
        using var context = NewContext();
        var store = new DbFirstUserStore(context);
        var user = new User { Username = "teacher1", PasswordHash = "hash", Role = Roles.Teacher, DisplayName = "Meera", DateOfBirth = new DateOnly(1985, 3, 1) };

        store.Add(user);

        Assert.True(user.Id > 0);
        Assert.Equal("Meera", store.FindByUsername("teacher1")!.DisplayName);
        Assert.Equal(new DateOnly(1985, 3, 1), store.FindByUsername("teacher1")!.DateOfBirth);
        Assert.Equal("hash", store.FindByUsername("Teacher1")!.PasswordHash);
        Assert.Null(store.FindByUsername("nobody"));
        Assert.Throws<InvalidOperationException>(() => store.Add(new User { Username = "TEACHER1", PasswordHash = "x", Role = Roles.Teacher }));
    }
}
