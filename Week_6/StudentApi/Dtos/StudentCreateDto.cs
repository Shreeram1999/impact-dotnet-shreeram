using System.ComponentModel.DataAnnotations;

namespace StudentApi.Dtos;

// Task 5.6/5.7 - the shape a client is allowed to SEND. Every attribute
// here is checked automatically before StudentsController.Create/Update
// ever runs, because the controller carries [ApiController] (see
// Validation_Notes.md for what happens without it) - an invalid body never
// reaches the Service layer at all, it gets a 400 straight from the
// framework.
public class StudentCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(5, 100)]
    public int Age { get; set; }

    [Required]
    public string RollNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Score { get; set; }
}
