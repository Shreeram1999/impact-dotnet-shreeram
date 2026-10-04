using StudentApi.Models;

namespace StudentApi.Services;

// Task 5.3/5.4 - what StudentsController is allowed to talk to. Note what's
// NOT here: no ModelState, no HttpContext, no status codes - this interface
// has no idea it's being called from a Web API at all. Add/Update return
// OperationResult(<Student>) so the Service - not the Controller - decides
// whether a request succeeded, hit a missing Id, or collided with an
// existing roll number (Task 5.5's Conflict case).
public interface IStudentService
{
    IEnumerable<Student> GetAll();
    Student? GetById(int id);
    IEnumerable<Student> Search(string? name);
    OperationResult<Student> Add(Student student);
    OperationResult Update(int id, Student student);
    OperationResult Delete(int id);
}
