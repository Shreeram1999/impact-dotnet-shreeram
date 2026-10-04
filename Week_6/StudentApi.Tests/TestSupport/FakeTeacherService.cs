using StudentApi.Models;
using StudentApi.Services;

namespace StudentApi.Tests.TestSupport;

// Task 5.10 (stretch) - the same in-memory test double role FakeStudentService
// plays, for TeachersController's own controller tests.
public class FakeTeacherService : ITeacherService
{
    private readonly List<Teacher> teachers = new();
    private int nextId = 1;

    public IEnumerable<Teacher> GetAll() => teachers;

    public Teacher? GetById(int id) => teachers.FirstOrDefault(t => t.Id == id);

    public OperationResult<Teacher> Add(Teacher teacher)
    {
        teacher.Id = nextId++;
        teachers.Add(teacher);
        return OperationResult<Teacher>.Ok(teacher, "Added.");
    }

    public OperationResult Update(int id, Teacher teacher)
    {
        var existing = GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No teacher found with Id {id}.");

        existing.Name = teacher.Name;
        existing.Email = teacher.Email;
        existing.Designation = teacher.Designation;
        return OperationResult.Ok("Updated.");
    }

    public OperationResult Delete(int id)
    {
        var existing = GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No teacher found with Id {id}.");

        teachers.Remove(existing);
        return OperationResult.Ok("Deleted.");
    }
}
