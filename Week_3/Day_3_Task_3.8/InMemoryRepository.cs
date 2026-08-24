// A single generic implementation of IRepository<T>, storing everything in
// a plain List<T> in memory - this is our stand-in for "a real database"
// so the whole project runs without needing one. `where T : class, IEntity,
// new()` is the same generic constraint idea from Week 2's Repository<T>:
// T must be a reference type, must have an Id (via IEntity), and must have
// a parameterless constructor so we can build a fresh one when needed.
public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity, new()
{
    private readonly List<T> items = new();
    private int nextId = 1;

    public IEnumerable<T> GetAll() => items;

    public T? GetById(int id) => items.FirstOrDefault(i => i.Id == id);

    public void Add(T entity)
    {
        if (entity.Id == 0)
            entity.Id = nextId++;

        items.Add(entity);
    }

    public bool Update(T entity)
    {
        var index = items.FindIndex(i => i.Id == entity.Id);
        if (index < 0)
            return false;

        items[index] = entity;
        return true;
    }

    public bool Delete(int id)
    {
        var entity = GetById(id);
        return entity is not null && items.Remove(entity);
    }
}

// StudentRepository and CourseRepository below don't add any NEW code -
// they just give the generic InMemoryRepository<T> a specific, friendly
// name for each entity type. This is what the task means by "StudentRepository,
// CourseRepository" as concrete classes: from the outside, callers see
// IRepository<Student> and IRepository<Course>, both backed by the exact
// same generic logic above.
public class StudentRepository : InMemoryRepository<Student>
{
}

public class CourseRepository : InMemoryRepository<Course>
{
}
