using Academics.Api.Data;
using Academics.Api.Models;

namespace Academics.Api.Services;

public interface ICourseService
{
    IEnumerable<Course> GetAll();
    Course? GetById(int id);
    OperationResult<Course> Add(Course course);
    OperationResult Update(int id, Course course);
    OperationResult Delete(int id);
    OperationResult Enroll(int courseId, int studentId, DateOnly today);
    OperationResult Unenroll(int courseId, int studentId);
    IEnumerable<Student>? StudentsIn(int courseId);
}

// Task 10.3 - Course rules, same style as StudentService: every decision is
// an OperationOutcome the controller maps to a status code.
//   NotFound -> 404 (the course in the URL doesn't exist)
//   Invalid  -> 400 (a referenced teacher/student in the BODY doesn't exist)
//   Conflict -> 409 (duplicate code, already enrolled)
public class CourseService : ICourseService
{
    private readonly IRepository<Course> courses;
    private readonly IRepository<Teacher> teachers;
    private readonly IRepository<Student> students;
    private readonly IEnrollmentRepository enrollments;

    public CourseService(IRepository<Course> courses, IRepository<Teacher> teachers, IRepository<Student> students, IEnrollmentRepository enrollments)
    {
        this.courses = courses;
        this.teachers = teachers;
        this.students = students;
        this.enrollments = enrollments;
    }

    public IEnumerable<Course> GetAll() => courses.GetAll();

    public Course? GetById(int id) => courses.GetById(id);

    public OperationResult<Course> Add(Course course)
    {
        if (courses.GetAll().Any(c => string.Equals(c.Code, course.Code, StringComparison.OrdinalIgnoreCase)))
            return OperationResult<Course>.Conflict($"Course code '{course.Code}' is already in use.");

        if (course.TeacherId is { } teacherId && teachers.GetById(teacherId) is null)
            return OperationResult<Course>.Invalid($"No teacher found with Id {teacherId}.");

        courses.Add(course);
        return OperationResult<Course>.Ok(course, $"Course '{course.Code}' added successfully.");
    }

    public OperationResult Update(int id, Course course)
    {
        var existing = courses.GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No course found with Id {id}.");

        if (courses.GetAll().Any(c => c.Id != id && string.Equals(c.Code, course.Code, StringComparison.OrdinalIgnoreCase)))
            return OperationResult.Conflict($"Course code '{course.Code}' is already in use.");

        if (course.TeacherId is { } teacherId && teachers.GetById(teacherId) is null)
            return OperationResult.Invalid($"No teacher found with Id {teacherId}.");

        existing.Code = course.Code;
        existing.Title = course.Title;
        existing.Credits = course.Credits;
        existing.TeacherId = course.TeacherId;
        courses.Update(existing);
        return OperationResult.Ok($"Course '{existing.Code}' updated successfully.");
    }

    public OperationResult Delete(int id)
    {
        var existing = courses.GetById(id);
        if (existing is null)
            return OperationResult.NotFound($"No course found with Id {id}.");

        courses.Delete(id);
        return OperationResult.Ok($"Course '{existing.Code}' deleted successfully.");
    }

    public OperationResult Enroll(int courseId, int studentId, DateOnly today)
    {
        if (courses.GetById(courseId) is null)
            return OperationResult.NotFound($"No course found with Id {courseId}.");

        if (students.GetById(studentId) is null)
            return OperationResult.Invalid($"No student found with Id {studentId}.");

        if (enrollments.Exists(studentId, courseId))
            return OperationResult.Conflict($"Student {studentId} is already enrolled in course {courseId}.");

        enrollments.Add(new Enrollment { StudentId = studentId, CourseId = courseId, EnrolledOn = today });
        return OperationResult.Ok($"Student {studentId} enrolled in course {courseId}.");
    }

    public OperationResult Unenroll(int courseId, int studentId) =>
        enrollments.Remove(studentId, courseId)
            ? OperationResult.Ok($"Student {studentId} removed from course {courseId}.")
            : OperationResult.NotFound($"Student {studentId} is not enrolled in course {courseId}.");

    public IEnumerable<Student>? StudentsIn(int courseId)
    {
        if (courses.GetById(courseId) is null)
            return null;

        return enrollments.StudentIdsFor(courseId).Select(students.GetById).OfType<Student>().ToList();
    }
}
