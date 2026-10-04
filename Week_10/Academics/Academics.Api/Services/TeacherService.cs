using Academics.Api.Data;
using Academics.Api.Models;

namespace Academics.Api.Services;

// Task 5.10 (stretch) - the format-level rules (Required, EmailAddress) live
// on TeacherCreateDto, same division of labor as Student. Week 7 adds the
// one cross-record rule the new unique Teachers.Email index needs: a
// duplicate email is a 409 here rather than a database error (500).
public class TeacherService : ITeacherService
{
    private readonly IRepository<Teacher> repository;

    public TeacherService(IRepository<Teacher> repository)
    {
        this.repository = repository;
    }

    public IEnumerable<Teacher> GetAll() => repository.GetAll();

    public Teacher? GetById(int id) => repository.GetById(id);

    public OperationResult<Teacher> Add(Teacher teacher)
    {
        if (repository.GetAll().Any(t => string.Equals(t.Email, teacher.Email, StringComparison.OrdinalIgnoreCase)))
            return OperationResult<Teacher>.Conflict($"Email '{teacher.Email}' is already in use.");

        repository.Add(teacher);
        return OperationResult<Teacher>.Ok(teacher, $"Teacher '{teacher.Name}' added successfully.");
    }

    public OperationResult Update(int id, Teacher teacher)
    {
        var existing = repository.GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No teacher found with Id {id}.");

        if (repository.GetAll().Any(t => t.Id != id && string.Equals(t.Email, teacher.Email, StringComparison.OrdinalIgnoreCase)))
            return OperationResult.Conflict($"Email '{teacher.Email}' is already in use by another teacher.");

        existing.Name = teacher.Name;
        existing.Email = teacher.Email;
        existing.Designation = teacher.Designation;

        repository.Update(existing);
        return OperationResult.Ok($"Teacher '{existing.Name}' updated successfully.");
    }

    public OperationResult Delete(int id)
    {
        var existing = repository.GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No teacher found with Id {id}.");

        repository.Delete(id);
        return OperationResult.Ok($"Teacher '{existing.Name}' deleted successfully.");
    }
}
