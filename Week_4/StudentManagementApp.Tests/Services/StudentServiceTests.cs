using Moq;
using StudentManagementApp.Data;
using StudentManagementApp.Models;
using StudentManagementApp.Services;

namespace StudentManagementApp.Tests.Services;

// This is the ≥80% coverage target from the Week 4 Testing Focus: the
// service layer, with IRepository<Student> mocked via Moq so these tests
// never depend on InMemoryRepository's own behavior (that's covered
// separately by InMemoryRepositoryTests).
public class StudentServiceTests
{
    private static Mock<IRepository<Student>> MockRepositoryWithStudents(params Student[] existingStudents)
    {
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetAll()).Returns(existingStudents);
        return mockRepository;
    }

    [Fact]
    public void AddStudent_ValidStudent_SucceedsAndCallsRepositoryAdd()
    {
        var mockRepository = MockRepositoryWithStudents();
        var service = new StudentService(mockRepository.Object, new TransactionLog());

        var result = service.AddStudent("Asha", 20, "R1", "asha@example.com");

        Assert.True(result.Success);
        mockRepository.Verify(r => r.Add(It.Is<Student>(s => s.Name == "Asha" && s.RollNumber == "R1")), Times.Once);
    }

    [Fact]
    public void AddStudent_DuplicateRollNumber_IsRejected()
    {
        var existing = new Student { Id = 1, Name = "Rohit", Age = 21, RollNumber = "R1", Email = "rohit@example.com" };
        var mockRepository = MockRepositoryWithStudents(existing);
        var service = new StudentService(mockRepository.Object, new TransactionLog());

        var result = service.AddStudent("Asha", 20, "R1", "asha@example.com");

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Add(It.IsAny<Student>()), Times.Never);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(101)]
    public void AddStudent_InvalidAge_IsRejected(int invalidAge)
    {
        var mockRepository = MockRepositoryWithStudents();
        var service = new StudentService(mockRepository.Object, new TransactionLog());

        var result = service.AddStudent("Asha", invalidAge, "R1", "asha@example.com");

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Add(It.IsAny<Student>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddStudent_EmptyName_IsRejected(string invalidName)
    {
        var mockRepository = MockRepositoryWithStudents();
        var service = new StudentService(mockRepository.Object, new TransactionLog());

        var result = service.AddStudent(invalidName, 20, "R1", "asha@example.com");

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Add(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void UpdateStudent_MissingId_FailsCleanly()
    {
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(999)).Returns((Student?)null);
        var service = new StudentService(mockRepository.Object, new TransactionLog());

        var result = service.UpdateStudent(999, "Nobody", 20, "R9", "nobody@example.com");

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Update(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void DeleteStudent_MissingId_FailsCleanly()
    {
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(999)).Returns((Student?)null);
        var service = new StudentService(mockRepository.Object, new TransactionLog());

        var result = service.DeleteStudent(999);

        Assert.False(result.Success);
        mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void AddStudent_Success_RecordsEntryInTransactionLog()
    {
        var mockRepository = MockRepositoryWithStudents();
        var transactionLog = new TransactionLog();
        var service = new StudentService(mockRepository.Object, transactionLog);

        service.AddStudent("Asha", 20, "R1", "asha@example.com");

        Assert.Single(transactionLog.History);
        Assert.Contains("Asha", transactionLog.History[0]);
    }

    [Fact]
    public void AddStudent_Rejected_DoesNotRecordAnyTransactionLogEntry()
    {
        var mockRepository = MockRepositoryWithStudents();
        var transactionLog = new TransactionLog();
        var service = new StudentService(mockRepository.Object, transactionLog);

        service.AddStudent("Asha", 200, "R1", "asha@example.com");

        Assert.Empty(transactionLog.History);
    }

    [Fact]
    public void UpdateStudent_Success_RecordsEntryInTransactionLog()
    {
        var existing = new Student { Id = 1, Name = "Rohit", Age = 21, RollNumber = "R1", Email = "rohit@example.com" };
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        mockRepository.Setup(r => r.GetAll()).Returns(new[] { existing });
        var transactionLog = new TransactionLog();
        var service = new StudentService(mockRepository.Object, transactionLog);

        var result = service.UpdateStudent(1, "Rohit Updated", 22, "R1", "rohit@example.com");

        Assert.True(result.Success);
        Assert.Single(transactionLog.History);
        Assert.Contains("Rohit Updated", transactionLog.History[0]);
    }

    [Fact]
    public void DeleteStudent_Success_RecordsEntryInTransactionLog()
    {
        var existing = new Student { Id = 1, Name = "Rohit", Age = 21, RollNumber = "R1", Email = "rohit@example.com" };
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        var transactionLog = new TransactionLog();
        var service = new StudentService(mockRepository.Object, transactionLog);

        var result = service.DeleteStudent(1);

        Assert.True(result.Success);
        Assert.Single(transactionLog.History);
        Assert.Contains("Rohit", transactionLog.History[0]);
    }
}
