namespace StudentManagementApp.Data;

// Same contract Week 3's Repository task (Day_3_Task_3.8) used: anything that
// wants to live inside an InMemoryRepository<T> just needs an Id. Student and
// Teacher (in /Models) both implement this so the SAME generic repository can
// store either one.
public interface IEntity
{
    int Id { get; set; }
}
