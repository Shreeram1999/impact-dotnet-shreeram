using StudentManagementApp.Data;

namespace StudentManagementApp.Models;

// Task 4.1 - the Teacher model. No age field, so no numeric range to
// enforce here - but it's still IEntity, which is the only thing
// InMemoryRepository<T> actually requires, so it slots into the exact same
// /Data layer Student uses without either model knowing the other exists.
public class Teacher : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;

    public override string ToString() => $"Teacher #{Id}: {Name} ({Designation}, {Email})";
}
