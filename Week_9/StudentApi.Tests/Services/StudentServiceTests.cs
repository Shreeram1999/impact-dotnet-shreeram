using Moq;
using StudentApi.Data;
using StudentApi.Models;
using StudentApi.Services;

namespace StudentApi.Tests.Services;

// The Testing Focus target for this week: the Service, with IRepository<T>
// mocked via Moq so these tests never depend on InMemoryRepository's own
// locking/storage behavior.
public class StudentServiceTests
{
    private static Mock<IRepository<Student>> MockRepositoryWithStudents(params Student[] existingStudents)
    {
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetAll()).Returns(existingStudents);
        return mockRepository;
    }

    [Fact]
    public void Add_ValidStudent_SucceedsAndReturnsTheAddedStudent()
    {
        var mockRepository = MockRepositoryWithStudents();
        var service = new StudentService(mockRepository.Object);

        var result = service.Add(new Student { Name = "Asha", Age = 20, RollNumber = "R1", Email = "asha@example.com" });

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        Assert.Equal("Asha", result.Value!.Name);
        mockRepository.Verify(r => r.Add(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public void Add_DuplicateRollNumber_ReturnsConflict()
    {
        var existing = new Student { Id = 1, Name = "Rohit", RollNumber = "R1" };
        var mockRepository = MockRepositoryWithStudents(existing);
        var service = new StudentService(mockRepository.Object);

        var result = service.Add(new Student { Name = "Asha", RollNumber = "R1" });

        Assert.Equal(OperationOutcome.Conflict, result.Outcome);
        mockRepository.Verify(r => r.Add(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void Update_MissingId_ReturnsNotFound()
    {
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(999)).Returns((Student?)null);
        var service = new StudentService(mockRepository.Object);

        var result = service.Update(999, new Student { Name = "Nobody", RollNumber = "R9" });

        Assert.Equal(OperationOutcome.NotFound, result.Outcome);
        mockRepository.Verify(r => r.Update(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void Update_RollNumberTakenByAnotherStudent_ReturnsConflict()
    {
        var existing = new Student { Id = 1, Name = "Asha", RollNumber = "R1" };
        var other = new Student { Id = 2, Name = "Rohit", RollNumber = "R2" };
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        mockRepository.Setup(r => r.GetAll()).Returns(new[] { existing, other });
        var service = new StudentService(mockRepository.Object);

        var result = service.Update(1, new Student { Name = "Asha", RollNumber = "R2" });

        Assert.Equal(OperationOutcome.Conflict, result.Outcome);
        mockRepository.Verify(r => r.Update(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void Update_KeepingItsOwnRollNumber_Succeeds()
    {
        var existing = new Student { Id = 1, Name = "Asha", RollNumber = "R1" };
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        mockRepository.Setup(r => r.GetAll()).Returns(new[] { existing });
        var service = new StudentService(mockRepository.Object);

        var result = service.Update(1, new Student { Name = "Asha Updated", RollNumber = "R1", Age = 22 });

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        Assert.Equal("Asha Updated", existing.Name);
    }

    [Fact]
    public void Update_DoesNotOverwriteInternalNotes()
    {
        var existing = new Student { Id = 1, Name = "Asha", RollNumber = "R1", InternalNotes = "flagged for review" };
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        mockRepository.Setup(r => r.GetAll()).Returns(new[] { existing });
        var service = new StudentService(mockRepository.Object);

        service.Update(1, new Student { Name = "Asha Updated", RollNumber = "R1" });

        Assert.Equal("flagged for review", existing.InternalNotes);
    }

    [Fact]
    public void Delete_MissingId_ReturnsNotFound()
    {
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(999)).Returns((Student?)null);
        var service = new StudentService(mockRepository.Object);

        var result = service.Delete(999);

        Assert.Equal(OperationOutcome.NotFound, result.Outcome);
        mockRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Delete_ExistingId_Succeeds()
    {
        var existing = new Student { Id = 1, Name = "Asha" };
        var mockRepository = new Mock<IRepository<Student>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        var service = new StudentService(mockRepository.Object);

        var result = service.Delete(1);

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        mockRepository.Verify(r => r.Delete(1), Times.Once);
    }

    // Task 5.9 - Search: hit, miss, and empty query, per the Testing Focus.
    [Fact]
    public void Search_MatchingNameCaseInsensitive_ReturnsHit()
    {
        var students = new[]
        {
            new Student { Id = 1, Name = "Asha" },
            new Student { Id = 2, Name = "Rohit" }
        };
        var mockRepository = MockRepositoryWithStudents(students);
        var service = new StudentService(mockRepository.Object);

        var result = service.Search("ASH").ToList();

        Assert.Single(result);
        Assert.Equal("Asha", result[0].Name);
    }

    [Fact]
    public void Search_NoMatch_ReturnsEmpty()
    {
        var students = new[] { new Student { Id = 1, Name = "Asha" } };
        var mockRepository = MockRepositoryWithStudents(students);
        var service = new StudentService(mockRepository.Object);

        var result = service.Search("zzz");

        Assert.Empty(result);
    }

    [Fact]
    public void Search_EmptyQuery_ReturnsEveryStudent()
    {
        var students = new[]
        {
            new Student { Id = 1, Name = "Asha" },
            new Student { Id = 2, Name = "Rohit" }
        };
        var mockRepository = MockRepositoryWithStudents(students);
        var service = new StudentService(mockRepository.Object);

        var result = service.Search(string.Empty);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public void Search_NullQuery_ReturnsEveryStudent()
    {
        var students = new[] { new Student { Id = 1, Name = "Asha" } };
        var mockRepository = MockRepositoryWithStudents(students);
        var service = new StudentService(mockRepository.Object);

        var result = service.Search(null);

        Assert.Single(result);
    }
}
