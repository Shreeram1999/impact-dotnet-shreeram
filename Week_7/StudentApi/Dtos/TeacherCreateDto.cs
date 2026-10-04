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
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Designation { get; set; } = string.Empty;
}
