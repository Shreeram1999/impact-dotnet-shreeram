using System.ComponentModel.DataAnnotations;

namespace Academics.Api.Dtos;

// Task 5.6/5.7 - the shape a client is allowed to SEND. Every attribute
// here is checked automatically before StudentsController.Create/Update
// ever runs, because the controller carries [ApiController] (see
// Week_5/StudentApi/Validation_Notes.md) - an invalid body never reaches
// the Service layer at all, it gets a 400 straight from the framework.
//
// Week 7 - every StringLength matches its database column length, so an
// over-long value is a 400 here instead of a SQL truncation error (500).
public class StudentCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(5, 100)]
    public int Age { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string RollNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Score { get; set; }

    // Task 7.8 - optional; the column is nullable.
    public DateOnly? EnrolledOn { get; set; }
}
