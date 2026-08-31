namespace StudentApi.Data;

// Task 5.3 - the Repository pattern behind the Service layer, unchanged in
// shape from Weeks 3-4. The API never talks to this interface directly -
// only IStudentService/ITeacherService do - which is what keeps the storage
// mechanism (an in-memory list today) swappable without touching a single
// controller or DTO.
public interface IRepository<T> where T : class, IEntity
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    void Add(T entity);
    bool Update(T entity);
    bool Delete(int id);
}
