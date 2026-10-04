using Academics.Api.Data;

namespace Academics.Api.Models;

// Task 10.3 - the new Academics entity: a course, optionally led by a
// teacher, with students enrolled in it (many-to-many via Enrollment).
public class Course : IEntity
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Credits { get; set; }
    public int? TeacherId { get; set; }

    public List<Enrollment> Enrollments { get; set; } = [];
}

// The join entity. Composite key (StudentId, CourseId) - a student can be
// in a course at most once.
public class Enrollment
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateOnly EnrolledOn { get; set; }
}
