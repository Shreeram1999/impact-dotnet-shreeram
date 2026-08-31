namespace StudentApi.Data;

// Task 5.3 - one generic implementation, backed by a plain List<T>, standing
// in for "a real database" until Week 7. Registered AddSingleton in
// Program.cs, which means - unlike the Week 4 console app - this store can
// now be hit by several concurrent HTTP requests at once, so mutations are
// guarded by a lock. That's the one behavioral change from the Week 3/4
// version; everything else about the class is identical.
public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity, new()
{
    private readonly List<T> items = new();
    private readonly object gate = new();
    private int nextId = 1;

    public IEnumerable<T> GetAll()
    {
        lock (gate)
            return items.ToList();
    }

    public T? GetById(int id)
    {
        lock (gate)
            return items.FirstOrDefault(i => i.Id == id);
    }

    public void Add(T entity)
    {
        lock (gate)
        {
            if (entity.Id == 0)
                entity.Id = nextId++;

            items.Add(entity);
        }
    }

    public bool Update(T entity)
    {
        lock (gate)
        {
            var index = items.FindIndex(i => i.Id == entity.Id);
            if (index < 0)
                return false;

            items[index] = entity;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (gate)
        {
            var entity = items.FirstOrDefault(i => i.Id == id);
            return entity is not null && items.Remove(entity);
        }
    }
}
