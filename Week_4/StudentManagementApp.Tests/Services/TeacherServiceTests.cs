using Moq;
using StudentManagementApp.Data;
using StudentManagementApp.Models;
using StudentManagementApp.Services;

namespace StudentManagementApp.Tests.Services;

// Task 4.10 (stretch) - a smaller mirror of StudentServiceTests, proving the
// Teacher slice follows the same mocked-repository testing approach.
public class TeacherServiceTests
{
    [Fact]
    public void AddTeacher_ValidTeacher_SucceedsAndCallsRepositoryAdd()
    {
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetAll()).Returns(Array.Empty<Teacher>());
        var service = new TeacherService(mockRepository.Object, new TransactionLog());

        var result = service.AddTeacher("Dr. Iyer", "iyer@example.com", "Professor");

        Assert.True(result.Success);
        mockRepository.Verify(r => r.Add(It.Is<Teacher>(t => t.Name == "Dr. Iyer")), Times.Once);
    }

    [Fact]
    public void AddTeacher_EmptyName_IsRejected()
    {
        var mockRepository = new Mock<IRepository<Teacher>>();
        var service = new TeacherService(mockRepository.Object, new TransactionLog());

        var result = service.AddTeacher("   ", "iyer@example.com", "Professor");

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Add(It.IsAny<Teacher>()), Times.Never);
    }

    [Fact]
    public void AddTeacher_EmptyEmail_IsRejected()
    {
        var mockRepository = new Mock<IRepository<Teacher>>();
        var service = new TeacherService(mockRepository.Object, new TransactionLog());

        var result = service.AddTeacher("Dr. Iyer", "   ", "Professor");

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Add(It.IsAny<Teacher>()), Times.Never);
    }

    [Fact]
    public void GetAll_ReturnsEveryTeacherFromTheRepository()
    {
        var teachers = new[] { new Teacher { Id = 1, Name = "Dr. Iyer" } };
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetAll()).Returns(teachers);
        var service = new TeacherService(mockRepository.Object, new TransactionLog());

        Assert.Same(teachers, service.GetAll());
    }

    [Fact]
    public void GetById_DelegatesDirectlyToTheRepository()
    {
        var teacher = new Teacher { Id = 1, Name = "Dr. Iyer" };
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(teacher);
        var service = new TeacherService(mockRepository.Object, new TransactionLog());

        Assert.Same(teacher, service.GetById(1));
    }

    [Fact]
    public void UpdateTeacher_ExistingTeacher_SucceedsAndRecordsInTransactionLog()
    {
        var existing = new Teacher { Id = 1, Name = "Dr. Iyer", Email = "iyer@example.com", Designation = "Lecturer" };
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        var transactionLog = new TransactionLog();
        var service = new TeacherService(mockRepository.Object, transactionLog);

        var result = service.UpdateTeacher(1, "Dr. Iyer", "iyer@example.com", "Professor");

        Assert.True(result.Success);
        Assert.Equal("Professor", existing.Designation);
        mockRepository.Verify(r => r.Update(existing), Times.Once);
        Assert.Single(transactionLog.History);
    }

    [Fact]
    public void UpdateTeacher_MissingId_FailsCleanly()
    {
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetById(999)).Returns((Teacher?)null);
        var service = new TeacherService(mockRepository.Object, new TransactionLog());

        var result = service.UpdateTeacher(999, "Nobody", "nobody@example.com", "Nothing");

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Update(It.IsAny<Teacher>()), Times.Never);
    }

    [Fact]
    public void UpdateTeacher_EmptyName_IsRejected()
    {
        var existing = new Teacher { Id = 1, Name = "Dr. Iyer", Email = "iyer@example.com", Designation = "Lecturer" };
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        var service = new TeacherService(mockRepository.Object, new TransactionLog());

        var result = service.UpdateTeacher(1, "   ", "iyer@example.com", "Professor");

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Update(It.IsAny<Teacher>()), Times.Never);
    }

    [Fact]
    public void DeleteTeacher_MissingId_FailsCleanly()
    {
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetById(999)).Returns((Teacher?)null);
        var service = new TeacherService(mockRepository.Object, new TransactionLog());

        var result = service.DeleteTeacher(999);

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void AddTeacher_Success_SharesTheSameTransactionLogAsStudentService()
    {
        var mockStudentRepository = new Mock<IRepository<Student>>();
        mockStudentRepository.Setup(r => r.GetAll()).Returns(Array.Empty<Student>());
        var mockTeacherRepository = new Mock<IRepository<Teacher>>();
        mockTeacherRepository.Setup(r => r.GetAll()).Returns(Array.Empty<Teacher>());

        var sharedLog = new TransactionLog();
        var studentService = new StudentService(mockStudentRepository.Object, sharedLog);
        var teacherService = new TeacherService(mockTeacherRepository.Object, sharedLog);

        studentService.AddStudent("Asha", 20, "R1", "asha@example.com");
        teacherService.AddTeacher("Dr. Iyer", "iyer@example.com", "Professor");

        Assert.Equal(2, sharedLog.History.Count);
        Assert.Contains("Asha", sharedLog.History[0]);
        Assert.Contains("Dr. Iyer", sharedLog.History[1]);
    }
}
