using StudentApi.Models;

namespace StudentApi.Services;

// Task 5.10 (stretch) - same shape as IStudentService, mirrored for Teacher.
public interface ITeacherService
{
    IEnumerable<Teacher> GetAll();
    Teacher? GetById(int id);
    OperationResult<Teacher> Add(Teacher teacher);
    OperationResult Update(int id, Teacher teacher);
    OperationResult Delete(int id);
}
