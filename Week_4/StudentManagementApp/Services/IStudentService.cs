using StudentManagementApp.Models;

namespace StudentManagementApp.Services;

// Task 4.3 - all Student business rules live behind this interface. Add and
// Update take the raw field values (not a pre-built Student) on purpose:
// that's what lets StudentServiceTests construct an "invalid" request - like
// age 200 - and assert the SERVICE rejects it, without the Model's own
// setter already blocking that object from ever being built (see
// StudentService.AddStudent for how the two layers of validation fit
// together).
public interface IStudentService
{
    OperationResult AddStudent(string name, int age, string rollNumber, string email);
    IEnumerable<Student> GetAll();
    Student? GetById(int id);
    OperationResult UpdateStudent(int id, string name, int age, string rollNumber, string email);
    OperationResult DeleteStudent(int id);
}
