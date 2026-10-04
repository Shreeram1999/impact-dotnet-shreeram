using Moq;
using StudentApi.Data;
using StudentApi.Models;
using StudentApi.Services;

namespace StudentApi.Tests.Services;

// Week 7 - the database now enforces unique emails (Task 7.6), so both
// Services answer a duplicate with Conflict (-> 409) BEFORE the repository
// is ever called - otherwise the INSERT/UPDATE would fail inside SQL Server
// and surface as a 500. Repository mocked, as with every Service test.
public class EmailUniquenessTests
{
    private static readonly Student ExistingStudent = new() { Id = 1, Name = "Asha", RollNumber = "R1", Email = "asha@example.com" };
    private static readonly Teacher ExistingTeacher = new() { Id = 1, Name = "Dr. Iyer", Email = "iyer@example.com", Designation = "Maths" };

    [Fact]
    public void AddStudent_EmailAlreadyUsed_IgnoringCase_ReturnsConflict()
    {
        var repository = new Mock<IRepository<Student>>();
        repository.Setup(r => r.GetAll()).Returns([ExistingStudent]);

        var result = new StudentService(repository.Object).Add(new Student { Name = "Other", RollNumber = "R2", Email = "ASHA@example.com" });

        Assert.Equal(OperationOutcome.Conflict, result.Outcome);
        Assert.Contains("Email", result.Message);
        repository.Verify(r => r.Add(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void UpdateStudent_EmailUsedByAnotherStudent_ReturnsConflict()
    {
        var repository = new Mock<IRepository<Student>>();
        var target = new Student { Id = 2, Name = "Rohit", RollNumber = "R2", Email = "rohit@example.com" };
        repository.Setup(r => r.GetById(2)).Returns(target);
        repository.Setup(r => r.GetAll()).Returns([ExistingStudent, target]);

        var result = new StudentService(repository.Object).Update(2, new Student { Name = "Rohit", RollNumber = "R2", Email = "asha@example.com" });

        Assert.Equal(OperationOutcome.Conflict, result.Outcome);
        repository.Verify(r => r.Update(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void UpdateStudent_KeepingItsOwnEmail_Succeeds_AndCopiesEnrolledOn()
    {
        var repository = new Mock<IRepository<Student>>();
        var target = new Student { Id = 1, Name = "Asha", RollNumber = "R1", Email = "asha@example.com" };
        repository.Setup(r => r.GetById(1)).Returns(target);
        repository.Setup(r => r.GetAll()).Returns([target]);

        var result = new StudentService(repository.Object).Update(1,
            new Student { Name = "Asha K", RollNumber = "R1", Email = "asha@example.com", EnrolledOn = new DateOnly(2026, 9, 1) });

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        Assert.Equal(new DateOnly(2026, 9, 1), target.EnrolledOn);
    }

    [Fact]
    public void AddTeacher_EmailAlreadyUsed_ReturnsConflict()
    {
        var repository = new Mock<IRepository<Teacher>>();
        repository.Setup(r => r.GetAll()).Returns([ExistingTeacher]);

        var result = new TeacherService(repository.Object).Add(new Teacher { Name = "Other", Email = "Iyer@Example.com", Designation = "Physics" });

        Assert.Equal(OperationOutcome.Conflict, result.Outcome);
        repository.Verify(r => r.Add(It.IsAny<Teacher>()), Times.Never);
    }

    [Fact]
    public void UpdateTeacher_EmailUsedByAnotherTeacher_ReturnsConflict()
    {
        var repository = new Mock<IRepository<Teacher>>();
        var target = new Teacher { Id = 2, Name = "Rao", Email = "rao@example.com", Designation = "Physics" };
        repository.Setup(r => r.GetById(2)).Returns(target);
        repository.Setup(r => r.GetAll()).Returns([ExistingTeacher, target]);

        var result = new TeacherService(repository.Object).Update(2, new Teacher { Name = "Rao", Email = "iyer@example.com", Designation = "Physics" });

        Assert.Equal(OperationOutcome.Conflict, result.Outcome);
        repository.Verify(r => r.Update(It.IsAny<Teacher>()), Times.Never);
    }
}
