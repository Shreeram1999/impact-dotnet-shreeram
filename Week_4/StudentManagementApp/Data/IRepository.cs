namespace StudentManagementApp.Data;

// Task 4.2 - reusing Week 3's IRepository<T> (Day_3_Task_3.8) as-is.
//
// This is the /Data layer's half of the "folder structure as a contract"
// idea from this week's objective: nothing outside this folder is allowed
// to know HOW data is stored (a List<T> today, a real database from Week 7
// onward) - every other layer only ever talks to this interface.
public interface IRepository<T> where T : class, IEntity
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    void Add(T entity);
    bool Update(T entity);
    bool Delete(int id);
}
