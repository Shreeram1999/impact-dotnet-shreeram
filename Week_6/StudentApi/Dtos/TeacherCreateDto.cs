using System.ComponentModel.DataAnnotations;

namespace StudentApi.Dtos;

// Task 5.10 (stretch) - mirrors StudentCreateDto's role for Teacher.
public class TeacherCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Designation { get; set; } = string.Empty;
}
