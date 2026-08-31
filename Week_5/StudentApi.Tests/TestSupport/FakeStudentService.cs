using StudentApi.Models;
using StudentApi.Services;

namespace StudentApi.Tests.TestSupport;

// Task 5.5/5.6 - "a small set of controller tests asserting the right
// status code per outcome ... using an in-memory service" (Testing Focus).
// This is that in-memory service: a plain List<Student>-backed IStudentService
// test double, swapped in for the real DI registration in
// StudentsControllerTests via WebApplicationFactory, so those tests drive
// StudentsController through the REAL ASP.NET Core pipeline (including the
// [ApiController] automatic 400 behavior from Task 5.6) without depending
// on the real StudentService or InMemoryRepository at all.
public class FakeStudentService : IStudentService
{
    private readonly List<Student> students = new();
    private int nextId = 1;

    public IEnumerable<Student> GetAll() => students;

    public Student? GetById(int id) => students.FirstOrDefault(s => s.Id == id);

    public IEnumerable<Student> Search(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? students
            : students.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

    public OperationResult<Student> Add(Student student)
    {
        if (students.Any(s => s.RollNumber == student.RollNumber))
            return OperationResult<Student>.Conflict($"Roll number '{student.RollNumber}' is already in use.");

        student.Id = nextId++;
        students.Add(student);
        return OperationResult<Student>.Ok(student, "Added.");
    }

    public OperationResult Update(int id, Student student)
    {
        var existing = GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No student found with Id {id}.");

        existing.Name = student.Name;
        existing.Age = student.Age;
        existing.RollNumber = student.RollNumber;
        existing.Email = student.Email;
        existing.Score = student.Score;
        return OperationResult.Ok("Updated.");
    }

    public OperationResult Delete(int id)
    {
        var existing = GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No student found with Id {id}.");

        students.Remove(existing);
        return OperationResult.Ok("Deleted.");
    }
}
