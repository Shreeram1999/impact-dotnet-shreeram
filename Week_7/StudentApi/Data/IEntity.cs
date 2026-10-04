namespace StudentApi.Data;

// Same contract used since Week 3's Repository task and carried into Week 4:
// anything that wants to live in an InMemoryRepository<T> just needs an Id.
public interface IEntity
{
    int Id { get; set; }
}
