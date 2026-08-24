// Covers the full CRUD surface (GetAll/GetById/Add/Update/Delete) against
// the shared IRepository<T> interface - using StudentRepository as the
// concrete type, since it's really just InMemoryRepository<Student>
// underneath - plus the IUnitOfWork.Save() seam.
public class RepositoryTests
{
    [Fact]
    public void Add_AssignsAnId_AndItShowsUpInGetAll()
    {
        IRepository<Student> repository = new StudentRepository();

        repository.Add(new Student { Name = "Asha" });

        var all = repository.GetAll().ToList();
        Assert.Single(all);
        Assert.True(all[0].Id > 0);
        Assert.Equal("Asha", all[0].Name);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsMatchingEntity()
    {
        IRepository<Student> repository = new StudentRepository();
        repository.Add(new Student { Name = "Asha" });
        var addedId = repository.GetAll().First().Id;

        var found = repository.GetById(addedId);

        Assert.NotNull(found);
        Assert.Equal("Asha", found!.Name);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        IRepository<Student> repository = new StudentRepository();

        Assert.Null(repository.GetById(999));
    }

    [Fact]
    public void Update_ExistingEntity_ReplacesItAndReturnsTrue()
    {
        IRepository<Student> repository = new StudentRepository();
        repository.Add(new Student { Name = "Asha" });
        var id = repository.GetAll().First().Id;

        var updated = repository.Update(new Student { Id = id, Name = "Asha Updated" });

        Assert.True(updated);
        Assert.Equal("Asha Updated", repository.GetById(id)!.Name);
    }

    [Fact]
    public void Update_UnknownEntity_ReturnsFalse()
    {
        IRepository<Student> repository = new StudentRepository();

        var updated = repository.Update(new Student { Id = 999, Name = "Nobody" });

        Assert.False(updated);
    }

    [Fact]
    public void Delete_ExistingId_RemovesItAndReturnsTrue()
    {
        IRepository<Student> repository = new StudentRepository();
        repository.Add(new Student { Name = "Asha" });
        var id = repository.GetAll().First().Id;

        var deleted = repository.Delete(id);

        Assert.True(deleted);
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Delete_UnknownId_ReturnsFalse()
    {
        IRepository<Student> repository = new StudentRepository();

        Assert.False(repository.Delete(999));
    }

    [Fact]
    public void CourseRepository_WorksTheSameWayAsStudentRepository()
    {
        IRepository<Course> repository = new CourseRepository();

        repository.Add(new Course { Title = "Intro to C#" });

        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void UnitOfWork_Save_ReturnsTotalRecordCountAcrossBothRepositories()
    {
        IUnitOfWork unitOfWork = new UnitOfWork();
        unitOfWork.Students.Add(new Student { Name = "Asha" });
        unitOfWork.Students.Add(new Student { Name = "Rohit" });
        unitOfWork.Courses.Add(new Course { Title = "Intro to C#" });

        var savedCount = unitOfWork.Save();

        Assert.Equal(3, savedCount);
    }

    [Fact]
    public void UnitOfWork_Save_WithNoRecords_ReturnsZero()
    {
        IUnitOfWork unitOfWork = new UnitOfWork();

        Assert.Equal(0, unitOfWork.Save());
    }
}
