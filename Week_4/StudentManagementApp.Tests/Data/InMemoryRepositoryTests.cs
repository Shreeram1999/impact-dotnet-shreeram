using StudentManagementApp.Data;
using StudentManagementApp.Models;

namespace StudentManagementApp.Tests.Data;

// Task 4.2's "Done when: all five repo operations work" - covered here as a
// permanent xUnit test rather than the throwaway Main() the task describes
// for Day 1, plus the "seed 3 students" step it also asks for.
public class InMemoryRepositoryTests
{
    private static InMemoryRepository<Student> SeedWithThreeStudents()
    {
        var repository = new InMemoryRepository<Student>();
        repository.Add(new Student { Name = "Asha", Age = 20, RollNumber = "R1", Email = "asha@example.com" });
        repository.Add(new Student { Name = "Rohit", Age = 21, RollNumber = "R2", Email = "rohit@example.com" });
        repository.Add(new Student { Name = "Meera", Age = 19, RollNumber = "R3", Email = "meera@example.com" });
        return repository;
    }

    [Fact]
    public void GetAll_AfterSeeding_ReturnsAllThreeStudents()
    {
        var repository = SeedWithThreeStudents();

        Assert.Equal(3, repository.GetAll().Count());
    }

    [Fact]
    public void Add_AssignsAnIncrementingId()
    {
        var repository = SeedWithThreeStudents();

        var ids = repository.GetAll().Select(s => s.Id).ToList();

        Assert.Equal(new[] { 1, 2, 3 }, ids);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsMatchingStudent()
    {
        var repository = SeedWithThreeStudents();
        var firstId = repository.GetAll().First().Id;

        var found = repository.GetById(firstId);

        Assert.NotNull(found);
        Assert.Equal("Asha", found!.Name);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        var repository = SeedWithThreeStudents();

        Assert.Null(repository.GetById(999));
    }

    [Fact]
    public void Update_ExistingStudent_ReplacesItAndReturnsTrue()
    {
        var repository = SeedWithThreeStudents();
        var id = repository.GetAll().First().Id;

        var updated = repository.Update(new Student { Id = id, Name = "Asha Updated", Age = 22, RollNumber = "R1", Email = "asha@example.com" });

        Assert.True(updated);
        Assert.Equal("Asha Updated", repository.GetById(id)!.Name);
    }

    [Fact]
    public void Update_UnknownStudent_ReturnsFalse()
    {
        var repository = SeedWithThreeStudents();

        var updated = repository.Update(new Student { Id = 999, Name = "Nobody", Age = 20, RollNumber = "R9", Email = "n@example.com" });

        Assert.False(updated);
    }

    [Fact]
    public void Delete_ExistingId_RemovesItAndReturnsTrue()
    {
        var repository = SeedWithThreeStudents();
        var id = repository.GetAll().First().Id;

        var deleted = repository.Delete(id);

        Assert.True(deleted);
        Assert.Equal(2, repository.GetAll().Count());
    }

    [Fact]
    public void Delete_UnknownId_ReturnsFalse()
    {
        var repository = SeedWithThreeStudents();

        Assert.False(repository.Delete(999));
    }
}
