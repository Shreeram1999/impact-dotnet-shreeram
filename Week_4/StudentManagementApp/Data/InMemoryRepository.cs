namespace StudentManagementApp.Data;

// Task 4.2 - one generic implementation, backed by a plain List<T>, standing
// in for "a real database" so the whole app runs without needing one. Every
// entity (Student, Teacher, ...) gets its persistence for free just by
// implementing IEntity - this class never needs to know which entity it's
// actually holding.
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
