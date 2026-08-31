using Moq;
using StudentApi.Data;
using StudentApi.Models;
using StudentApi.Services;

namespace StudentApi.Tests.Services;

// Task 5.10 (stretch) - a smaller mirror of StudentServiceTests, proving
// the Teacher slice follows the same mocked-repository approach.
public class TeacherServiceTests
{
    [Fact]
    public void Add_ValidTeacher_Succeeds()
    {
        var mockRepository = new Mock<IRepository<Teacher>>();
        var service = new TeacherService(mockRepository.Object);

        var result = service.Add(new Teacher { Name = "Dr. Iyer", Email = "iyer@example.com", Designation = "Professor" });

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        mockRepository.Verify(r => r.Add(It.IsAny<Teacher>()), Times.Once);
    }

    [Fact]
    public void Update_MissingId_ReturnsNotFound()
    {
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetById(999)).Returns((Teacher?)null);
        var service = new TeacherService(mockRepository.Object);

        var result = service.Update(999, new Teacher { Name = "Nobody" });

        Assert.Equal(OperationOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public void Update_ExistingId_Succeeds()
    {
        var existing = new Teacher { Id = 1, Name = "Dr. Iyer", Email = "iyer@example.com", Designation = "Lecturer" };
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        var service = new TeacherService(mockRepository.Object);

        var result = service.Update(1, new Teacher { Name = "Dr. Iyer", Email = "iyer@example.com", Designation = "Professor" });

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        Assert.Equal("Professor", existing.Designation);
    }

    [Fact]
    public void Delete_MissingId_ReturnsNotFound()
    {
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetById(999)).Returns((Teacher?)null);
        var service = new TeacherService(mockRepository.Object);

        var result = service.Delete(999);

        Assert.Equal(OperationOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public void Delete_ExistingId_Succeeds()
    {
        var existing = new Teacher { Id = 1, Name = "Dr. Iyer" };
        var mockRepository = new Mock<IRepository<Teacher>>();
        mockRepository.Setup(r => r.GetById(1)).Returns(existing);
        var service = new TeacherService(mockRepository.Object);

        var result = service.Delete(1);

        Assert.Equal(OperationOutcome.Success, result.Outcome);
        mockRepository.Verify(r => r.Delete(1), Times.Once);
    }
}
