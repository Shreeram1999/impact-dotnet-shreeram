using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Tests.Data;

// The real InMemoryRepository<T> - StudentServiceTests and the controller
// tests all swap it out (Moq or a fake), so this is the one place its
// actual List<T> + lock implementation gets exercised directly.
public class InMemoryRepositoryTests
{
    [Fact]
    public void Add_AssignsAnIncrementingId()
    {
        var repository = new InMemoryRepository<Student>();

        repository.Add(new Student { Name = "Asha" });
        repository.Add(new Student { Name = "Rohit" });

        Assert.Equal(new[] { 1, 2 }, repository.GetAll().Select(s => s.Id));
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        var repository = new InMemoryRepository<Student>();

        Assert.Null(repository.GetById(999));
    }

    [Fact]
    public void Update_ExistingEntity_ReplacesItAndReturnsTrue()
    {
        var repository = new InMemoryRepository<Student>();
        repository.Add(new Student { Name = "Asha" });
        var id = repository.GetAll().First().Id;

        var updated = repository.Update(new Student { Id = id, Name = "Asha Updated" });

        Assert.True(updated);
        Assert.Equal("Asha Updated", repository.GetById(id)!.Name);
    }

    [Fact]
    public void Update_UnknownEntity_ReturnsFalse()
    {
        var repository = new InMemoryRepository<Student>();

        Assert.False(repository.Update(new Student { Id = 999, Name = "Nobody" }));
    }

    [Fact]
    public void Delete_ExistingId_RemovesItAndReturnsTrue()
    {
        var repository = new InMemoryRepository<Student>();
        repository.Add(new Student { Name = "Asha" });
        var id = repository.GetAll().First().Id;

        Assert.True(repository.Delete(id));
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Delete_UnknownId_ReturnsFalse()
    {
        var repository = new InMemoryRepository<Student>();

        Assert.False(repository.Delete(999));
    }

    [Fact]
    public void GetAll_ReturnsASnapshot_NotALiveReferenceToInternalStorage()
    {
        var repository = new InMemoryRepository<Student>();
        repository.Add(new Student { Name = "Asha" });

        var snapshot = repository.GetAll();
        repository.Add(new Student { Name = "Rohit" });

        Assert.Single(snapshot);
    }

    [Fact]
    public void ConcurrentAdds_FromManyThreads_NeverLoseAnEntryOrDuplicateAnId()
    {
        var repository = new InMemoryRepository<Student>();

        Parallel.For(0, 200, i => repository.Add(new Student { Name = $"Student {i}" }));

        var all = repository.GetAll().ToList();
        Assert.Equal(200, all.Count);
        Assert.Equal(200, all.Select(s => s.Id).Distinct().Count());
    }
}
