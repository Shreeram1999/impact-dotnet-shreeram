namespace StudentApi.Dtos;

// Task 5.10 (stretch) - mirrors StudentReadDto's role for Teacher.
public class TeacherReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
}
