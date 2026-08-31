using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Services;

// Task 5.10 (stretch) - Teacher has no cross-record uniqueness rule in this
// week's spec, so this Service has nothing left to enforce beyond
// existence checks - the format-level rules (Required, EmailAddress) live
// on TeacherCreateDto instead, same division of labor as Student.
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
        repository.Add(teacher);
        return OperationResult<Teacher>.Ok(teacher, $"Teacher '{teacher.Name}' added successfully.");
    }

    public OperationResult Update(int id, Teacher teacher)
    {
        var existing = repository.GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No teacher found with Id {id}.");

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
