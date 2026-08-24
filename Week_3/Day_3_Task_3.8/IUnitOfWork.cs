// Task 3.8 - Unit of Work.
//
// "Unit of Work" sits one level above the repositories: instead of every
// piece of code creating its own separate StudentRepository/CourseRepository
// (which, with a real database, could mean separate, uncoordinated
// database connections), a Unit of Work bundles all the repositories you
// need together and gives you ONE place to call Save() when you're
// finished making changes. This models "everything I changed in this unit
// of work should be saved together" - the same idea as a database
// transaction.
public interface IUnitOfWork
{
    IRepository<Student> Students { get; }
    IRepository<Course> Courses { get; }

    // Returns however many records were "saved" - in a real database-backed
    // implementation (like Entity Framework's SaveChanges(), which this is
    // deliberately modeled after) this would be the number of rows actually
    // written to the database.
    int Save();
}

public class UnitOfWork : IUnitOfWork
{
    public IRepository<Student> Students { get; } = new StudentRepository();
    public IRepository<Course> Courses { get; } = new CourseRepository();

    public int Save()
    {
        // There's no real database here, so there's nothing to "flush" -
        // Save() just reports how many records exist across both
        // repositories right now, standing in for "rows affected".
        return Students.GetAll().Count() + Courses.GetAll().Count();
    }
}
