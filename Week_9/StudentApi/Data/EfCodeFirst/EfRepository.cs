using Microsoft.EntityFrameworkCore;

namespace StudentApi.Data.EfCodeFirst;

// Task 7.7 - IRepository<T> over EF Core. One generic class covers every
// entity because DbSet<T> already speaks "a table of T"; EfStudentRepository
// and EfTeacherRepository below exist only to give each registration a
// concrete, self-describing name.
//
// Each method saves immediately, matching the in-memory and ADO.NET
// implementations' "every call is its own unit of work" behaviour, so the
// Service layer can't tell which one it has.
public class EfRepository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly AppDbContext context;

    public EfRepository(AppDbContext context)
    {
        this.context = context;
    }

    // AsNoTracking for reads that are only going to be mapped to DTOs.
    public IEnumerable<T> GetAll() => context.Set<T>().AsNoTracking().OrderBy(e => e.Id).ToList();

    public T? GetById(int id) => context.Set<T>().Find(id);

    public void Add(T entity)
    {
        context.Set<T>().Add(entity);
        context.SaveChanges(); // fills in entity.Id from the IDENTITY column
    }

    public bool Update(T entity)
    {
        // The Service fetched `entity` through GetById (tracked) and edited
        // it, so in the normal path EF already knows what changed. An
        // untracked instance with an unknown Id reports "not found".
        if (context.Entry(entity).State == EntityState.Detached)
        {
            if (!context.Set<T>().AsNoTracking().Any(e => e.Id == entity.Id))
                return false;
            context.Set<T>().Update(entity);
        }

        context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var entity = context.Set<T>().Find(id);
        if (entity is null)
            return false;

        context.Set<T>().Remove(entity);
        context.SaveChanges();
        return true;
    }
}

// Task 7.7 - EfStudentRepository : IRepository<Student>, swapped in behind
// the API with no controller or service change.
public class EfStudentRepository : EfRepository<Models.Student>
{
    public EfStudentRepository(AppDbContext context) : base(context)
    {
    }
}

public class EfTeacherRepository : EfRepository<Models.Teacher>
{
    public EfTeacherRepository(AppDbContext context) : base(context)
    {
    }
}
