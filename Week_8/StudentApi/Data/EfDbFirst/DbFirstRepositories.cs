using Microsoft.EntityFrameworkCore;
using StudentApi.Auth;
using Db = StudentApi.Data.EfDbFirst.Entities;
using Domain = StudentApi.Models;

namespace StudentApi.Data.EfDbFirst;

// Task 7.10 - IRepository<Student> over the SCAFFOLDED context.
//
// The difference from Code First: here the database is the source of truth
// and Entities/*.cs are generated from it (Task 7.9 - re-run the scaffold
// command in README.md after a schema change; never hand-edit the generated
// files). The generated classes belong to the database's shape, not to our
// domain, so this repository maps between Db.Student and Domain.Student
// instead of handing generated types to the Service layer. If the other
// team renames a column, only the scaffold and this mapping change - the
// Services, DTOs and controllers don't.
public class DbFirstStudentRepository : IRepository<Domain.Student>
{
    private readonly StudentPortalDbFirstContext context;

    public DbFirstStudentRepository(StudentPortalDbFirstContext context)
    {
        this.context = context;
    }

    public IEnumerable<Domain.Student> GetAll() =>
        context.Students.AsNoTracking().OrderBy(s => s.Id).Select(ToDomainExpression).ToList();

    public Domain.Student? GetById(int id) =>
        context.Students.AsNoTracking().Where(s => s.Id == id).Select(ToDomainExpression).FirstOrDefault();

    public void Add(Domain.Student entity)
    {
        var row = new Db.Student();
        Copy(entity, row);
        context.Students.Add(row);
        context.SaveChanges();
        entity.Id = row.Id;
    }

    public bool Update(Domain.Student entity)
    {
        var row = context.Students.Find(entity.Id);
        if (row is null)
            return false;

        Copy(entity, row);
        context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var row = context.Students.Find(id);
        if (row is null)
            return false;

        context.Students.Remove(row);
        context.SaveChanges();
        return true;
    }

    // An expression (not a method) so EF translates the projection to SQL
    // and only selects the columns the domain model needs.
    private static readonly System.Linq.Expressions.Expression<Func<Db.Student, Domain.Student>> ToDomainExpression = s => new Domain.Student
    {
        Id = s.Id,
        Name = s.Name,
        Age = s.Age,
        RollNumber = s.RollNumber,
        Email = s.Email,
        Score = s.Score,
        InternalNotes = s.InternalNotes,
        EnrolledOn = s.EnrolledOn,
        UserId = s.UserId
    };

    private static void Copy(Domain.Student from, Db.Student to)
    {
        to.Name = from.Name;
        to.Age = from.Age;
        to.RollNumber = from.RollNumber;
        to.Email = from.Email;
        to.Score = from.Score;
        to.InternalNotes = from.InternalNotes;
        to.EnrolledOn = from.EnrolledOn;
        to.UserId = from.UserId;
    }
}

public class DbFirstTeacherRepository : IRepository<Domain.Teacher>
{
    private readonly StudentPortalDbFirstContext context;

    public DbFirstTeacherRepository(StudentPortalDbFirstContext context)
    {
        this.context = context;
    }

    public IEnumerable<Domain.Teacher> GetAll() =>
        context.Teachers.AsNoTracking().OrderBy(t => t.Id)
            .Select(t => new Domain.Teacher { Id = t.Id, Name = t.Name, Email = t.Email, Designation = t.Designation, UserId = t.UserId })
            .ToList();

    public Domain.Teacher? GetById(int id) =>
        context.Teachers.AsNoTracking().Where(t => t.Id == id)
            .Select(t => new Domain.Teacher { Id = t.Id, Name = t.Name, Email = t.Email, Designation = t.Designation, UserId = t.UserId })
            .FirstOrDefault();

    public void Add(Domain.Teacher entity)
    {
        var row = new Db.Teacher { Name = entity.Name, Email = entity.Email, Designation = entity.Designation, UserId = entity.UserId };
        context.Teachers.Add(row);
        context.SaveChanges();
        entity.Id = row.Id;
    }

    public bool Update(Domain.Teacher entity)
    {
        var row = context.Teachers.Find(entity.Id);
        if (row is null)
            return false;

        row.Name = entity.Name;
        row.Email = entity.Email;
        row.Designation = entity.Designation;
        row.UserId = entity.UserId;
        context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var row = context.Teachers.Find(id);
        if (row is null)
            return false;

        context.Teachers.Remove(row);
        context.SaveChanges();
        return true;
    }
}

public class DbFirstUserStore : IUserStore
{
    private readonly StudentPortalDbFirstContext context;

    public DbFirstUserStore(StudentPortalDbFirstContext context)
    {
        this.context = context;
    }

    public Domain.User? FindByUsername(string username)
    {
        var normalized = username.ToLower();
        return context.Users.AsNoTracking()
            .Where(u => u.Username.ToLower() == normalized)
            .Select(u => new Domain.User { Id = u.Id, Username = u.Username, PasswordHash = u.PasswordHash, Role = u.Role, DisplayName = u.DisplayName, DateOfBirth = u.DateOfBirth })
            .FirstOrDefault();
    }

    public void Add(Domain.User user)
    {
        if (FindByUsername(user.Username) is not null)
            throw new InvalidOperationException($"Username '{user.Username}' already exists.");

        var row = new Db.User { Username = user.Username, PasswordHash = user.PasswordHash, Role = user.Role, DisplayName = user.DisplayName, DateOfBirth = user.DateOfBirth };
        context.Users.Add(row);
        context.SaveChanges();
        user.Id = row.Id;
    }
}
