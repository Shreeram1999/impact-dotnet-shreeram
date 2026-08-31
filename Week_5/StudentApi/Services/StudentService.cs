using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Services;

// Task 5.3/5.4/5.9 - every Student business rule lives here, not in the
// controller and not in a DataAnnotation. Format-level checks (Required,
// Range(5,100), EmailAddress on StudentCreateDto) are handled automatically
// by [ApiController] before an action even runs (Task 5.6) - the one rule
// that's left for this class is the one DataAnnotations structurally can't
// express: "is this roll number already taken by a DIFFERENT student?",
// which needs to look across the whole collection, not just the one record
// being validated.
public class StudentService : IStudentService
{
    private readonly IRepository<Student> repository;

    public StudentService(IRepository<Student> repository)
    {
        this.repository = repository;
    }

    public IEnumerable<Student> GetAll() => repository.GetAll();

    public Student? GetById(int id) => repository.GetById(id);

    // Task 5.9 - case-insensitive LINQ filter. A null/empty name matches
    // everything, so GET api/students/search with no query still returns a
    // (possibly full) 200 list rather than a 400 - "empty query" is a valid
    // search, per the Testing Focus section.
    public IEnumerable<Student> Search(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return repository.GetAll();

        return repository.GetAll()
            .Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    public OperationResult<Student> Add(Student student)
    {
        if (repository.GetAll().Any(s => s.RollNumber == student.RollNumber))
            return OperationResult<Student>.Conflict($"Roll number '{student.RollNumber}' is already in use.");

        repository.Add(student);
        return OperationResult<Student>.Ok(student, $"Student '{student.Name}' added successfully.");
    }

    public OperationResult Update(int id, Student student)
    {
        var existing = repository.GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No student found with Id {id}.");

        if (repository.GetAll().Any(s => s.RollNumber == student.RollNumber && s.Id != id))
            return OperationResult.Conflict($"Roll number '{student.RollNumber}' is already in use by another student.");

        // Only the fields StudentCreateDto governs are copied across -
        // InternalNotes (never carried by a DTO, Task 5.7) is left exactly
        // as it already was on the stored entity.
        existing.Name = student.Name;
        existing.Age = student.Age;
        existing.RollNumber = student.RollNumber;
        existing.Email = student.Email;
        existing.Score = student.Score;

        repository.Update(existing);
        return OperationResult.Ok($"Student '{existing.Name}' updated successfully.");
    }

    public OperationResult Delete(int id)
    {
        var existing = repository.GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No student found with Id {id}.");

        repository.Delete(id);
        return OperationResult.Ok($"Student '{existing.Name}' deleted successfully.");
    }
}
