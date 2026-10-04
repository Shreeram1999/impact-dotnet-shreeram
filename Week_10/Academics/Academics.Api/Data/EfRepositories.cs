using Academics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Academics.Api.Data;

// Week 7's generic EF repository, now the only data layer Academics needs.
public class EfRepository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly AcademicsDbContext context;

    public EfRepository(AcademicsDbContext context)
    {
        this.context = context;
    }

    public IEnumerable<T> GetAll() => context.Set<T>().AsNoTracking().OrderBy(e => e.Id).ToList();

    public T? GetById(int id) => context.Set<T>().Find(id);

    public void Add(T entity)
    {
        context.Set<T>().Add(entity);
        context.SaveChanges();
    }

    public bool Update(T entity)
    {
        if (context.Entry(entity).State == EntityState.Detached)
        {
            // A different instance with the same Id may already be tracked
            // (e.g. loaded earlier in this request): copy the new values onto
            // it, since attaching a second instance with that key would throw.
            var tracked = context.Set<T>().Local.FirstOrDefault(e => e.Id == entity.Id);
            if (tracked is not null)
                context.Entry(tracked).CurrentValues.SetValues(entity);
            else if (context.Set<T>().AsNoTracking().Any(e => e.Id == entity.Id))
                context.Set<T>().Update(entity);
            else
                return false;
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

public interface IEnrollmentRepository
{
    bool Exists(int studentId, int courseId);
    void Add(Enrollment enrollment);
    bool Remove(int studentId, int courseId);
    IReadOnlyList<int> StudentIdsFor(int courseId);
}

// The join table has a composite key, so it doesn't fit IRepository<T>
// (one int Id); it gets its own tiny repository instead.
public class EfEnrollmentRepository : IEnrollmentRepository
{
    private readonly AcademicsDbContext context;

    public EfEnrollmentRepository(AcademicsDbContext context)
    {
        this.context = context;
    }

    public bool Exists(int studentId, int courseId) =>
        context.Enrollments.Any(e => e.StudentId == studentId && e.CourseId == courseId);

    public void Add(Enrollment enrollment)
    {
        context.Enrollments.Add(enrollment);
        context.SaveChanges();
    }

    public bool Remove(int studentId, int courseId)
    {
        var enrollment = context.Enrollments.Find(studentId, courseId);
        if (enrollment is null)
            return false;

        context.Enrollments.Remove(enrollment);
        context.SaveChanges();
        return true;
    }

    public IReadOnlyList<int> StudentIdsFor(int courseId) =>
        context.Enrollments.Where(e => e.CourseId == courseId).OrderBy(e => e.StudentId).Select(e => e.StudentId).ToList();
}
