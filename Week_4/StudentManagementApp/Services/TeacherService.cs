using StudentManagementApp.Data;
using StudentManagementApp.Models;

namespace StudentManagementApp.Services;

// Task 4.10 (stretch) - the Teacher end-to-end slice. Note it takes the SAME
// TransactionLog instance StudentService does (both are registered as
// Singletons, see Program.cs) - one shared log, in one shared order, for
// mutations across both entities.
public class TeacherService : ITeacherService
{
    private readonly IRepository<Teacher> repository;
    private readonly TransactionLog transactionLog;

    public TeacherService(IRepository<Teacher> repository, TransactionLog transactionLog)
    {
        this.repository = repository;
        this.transactionLog = transactionLog;
    }

    public OperationResult AddTeacher(string name, string email, string designation)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Fail("Teacher name cannot be empty.");

        if (string.IsNullOrWhiteSpace(email))
            return OperationResult.Fail("Teacher email cannot be empty.");

        var teacher = new Teacher { Name = name, Email = email, Designation = designation };
        repository.Add(teacher);
        transactionLog.Record($"Added teacher '{teacher.Name}' ({teacher.Designation}).");
        return OperationResult.Ok($"Teacher '{teacher.Name}' added successfully.");
    }

    public IEnumerable<Teacher> GetAll() => repository.GetAll();

    public Teacher? GetById(int id) => repository.GetById(id);

    public OperationResult UpdateTeacher(int id, string name, string email, string designation)
    {
        var existing = repository.GetById(id);
        if (existing is null)
            return OperationResult.Fail($"No teacher found with Id {id}.");

        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Fail("Teacher name cannot be empty.");

        if (string.IsNullOrWhiteSpace(email))
            return OperationResult.Fail("Teacher email cannot be empty.");

        existing.Name = name;
        existing.Email = email;
        existing.Designation = designation;

        repository.Update(existing);
        transactionLog.Record($"Updated teacher '{existing.Name}' (Id {id}).");
        return OperationResult.Ok($"Teacher '{existing.Name}' updated successfully.");
    }

    public OperationResult DeleteTeacher(int id)
    {
        var existing = repository.GetById(id);
        if (existing is null)
            return OperationResult.Fail($"No teacher found with Id {id}.");

        repository.Delete(id);
        transactionLog.Record($"Deleted teacher '{existing.Name}' (Id {id}).");
        return OperationResult.Ok($"Teacher '{existing.Name}' deleted successfully.");
    }
}
