using StudentApi.Data;

namespace StudentApi.Models;

// Task 5.3 - the entity. This is what the Repository and Service work with;
// it is never returned directly from a controller action - see
// Dtos/StudentMapper.cs for the entity<->DTO boundary.
//
// InternalNotes (Task 5.7) exists ONLY to prove that boundary: it's set by
// nothing in this API (no DTO exposes it), but if a mapping bug ever added
// it to StudentReadDto by accident, StudentMapperTests would catch it.
public class Student : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // 0-100 raw exam score - what Task 5.8's IGradeStrategy converts into a
    // percentage or GPA display on demand.
    public int Score { get; set; }

    // Deliberately internal-only administrative metadata - never exposed by
    // StudentCreateDto (can't be set by a client) or StudentReadDto (never
    // sent back).
    public string InternalNotes { get; set; } = string.Empty;
}
