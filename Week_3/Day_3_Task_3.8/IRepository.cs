// Task 3.8 - the Repository pattern.
//
// A "repository" hides how/where data is actually stored (a database, a
// file, or - as here, for a runnable demo - just an in-memory list) behind
// a simple, storage-agnostic interface: GetAll/GetById/Add/Update/Delete.
// Code that USES an IRepository<Student> doesn't know or care whether it's
// backed by SQL Server, a JSON file, or a plain List<Student> - and that's
// exactly the seam later weeks will plug a real database into, without
// having to change any code that already depends on IRepository<T>.
public interface IRepository<T> where T : class, IEntity
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    void Add(T entity);
    bool Update(T entity);
    bool Delete(int id);
}
