using StudentManagementApp.Models;

namespace StudentManagementApp.Services;

// Task 4.10 (stretch) - same shape as IStudentService, mirrored for Teacher.
public interface ITeacherService
{
    OperationResult AddTeacher(string name, string email, string designation);
    IEnumerable<Teacher> GetAll();
    Teacher? GetById(int id);
    OperationResult UpdateTeacher(int id, string name, string email, string designation);
    OperationResult DeleteTeacher(int id);
}
