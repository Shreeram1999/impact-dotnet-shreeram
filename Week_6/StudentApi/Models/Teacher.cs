using StudentApi.Data;

namespace StudentApi.Models;

// Task 5.10 (stretch) - the Teacher entity backing TeachersController.
public class Teacher : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
}
