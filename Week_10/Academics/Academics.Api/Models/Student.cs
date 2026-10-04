using Academics.Api.Data;

namespace Academics.Api.Models;

// The Student entity, carried forward from Weeks 5-9. Week 10 drops the
// UserId link: logins now live in a DIFFERENT service's database
// (IdentityDb), and a database can't hold a foreign key into another
// service's database - database-per-service means no cross-service joins.
public class Student : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // 0-100 raw exam score (Week 5's grading Strategy/Factory).
    public int Score { get; set; }

    // Internal-only; no DTO exposes it (Week 5, Task 5.7).
    public string InternalNotes { get; set; } = string.Empty;

    public DateOnly? EnrolledOn { get; set; }

    public List<Enrollment> Enrollments { get; set; } = [];
}
