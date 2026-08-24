using StudentManagementApp.Data;
using StudentManagementApp.Models;

namespace StudentManagementApp.Services;

// Task 4.3/4.4 - this is the layer the ≥80% coverage target is aimed at
// (see the Testing Focus section of the Week 4 doc). Every rule about what
// makes a Student "valid enough to save" lives HERE, not in the Controller
// and not in the View - that's the whole point of the layering this week
// is teaching.
public class StudentService : IStudentService
{
    private readonly IRepository<Student> repository;
    private readonly TransactionLog transactionLog;

    // Depending on IRepository<Student> (the interface) rather than
    // InMemoryRepository<Student> directly is what let Task 4.7 swap the
    // concrete repository by changing one line in Program.cs, and what lets
    // StudentServiceTests swap in a Moq mock instead of a real repository.
    public StudentService(IRepository<Student> repository, TransactionLog transactionLog)
    {
        this.repository = repository;
        this.transactionLog = transactionLog;
    }

    public OperationResult AddStudent(string name, int age, string rollNumber, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Fail("Student name cannot be empty.");

        if (repository.GetAll().Any(s => s.RollNumber == rollNumber))
            return OperationResult.Fail($"Roll number '{rollNumber}' is already in use.");

        Student student;
        try
        {
            // The Model (Student.Age's setter, Task 4.1) already enforces the
            // 5-100 range as an invariant of the type itself. The Service
            // doesn't duplicate that range check - it just catches the
            // Model's exception and turns it into the same OperationResult.Fail(...)
            // shape every other rejection uses, so the Controller/View never
            // need to care whether a failure came from the Model or the Service.
            student = new Student { Name = name, Age = age, RollNumber = rollNumber, Email = email };
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return OperationResult.Fail(ex.Message);
        }

        repository.Add(student);
        transactionLog.Record($"Added student '{student.Name}' (Roll {student.RollNumber}).");
        return OperationResult.Ok($"Student '{student.Name}' added successfully.");
    }

    public IEnumerable<Student> GetAll() => repository.GetAll();

    public Student? GetById(int id) => repository.GetById(id);

    public OperationResult UpdateStudent(int id, string name, int age, string rollNumber, string email)
    {
        var existing = repository.GetById(id);
        if (existing is null)
            return OperationResult.Fail($"No student found with Id {id}.");

        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Fail("Student name cannot be empty.");

        if (repository.GetAll().Any(s => s.RollNumber == rollNumber && s.Id != id))
            return OperationResult.Fail($"Roll number '{rollNumber}' is already in use by another student.");

        // Age is set first and alone: repository.GetById returns the SAME
        // object that's already sitting in the repository's list (not a
        // copy), so if Age's setter throws, it must throw before any other
        // field on `existing` has been touched - otherwise a rejected update
        // would still leave the stored student half-mutated.
        try
        {
            existing.Age = age;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return OperationResult.Fail(ex.Message);
        }

        existing.Name = name;
        existing.RollNumber = rollNumber;
        existing.Email = email;

        repository.Update(existing);
        transactionLog.Record($"Updated student '{existing.Name}' (Id {id}).");
        return OperationResult.Ok($"Student '{existing.Name}' updated successfully.");
    }

    public OperationResult DeleteStudent(int id)
    {
        var existing = repository.GetById(id);
        if (existing is null)
            return OperationResult.Fail($"No student found with Id {id}.");

        repository.Delete(id);
        transactionLog.Record($"Deleted student '{existing.Name}' (Id {id}).");
        return OperationResult.Ok($"Student '{existing.Name}' deleted successfully.");
    }
}
