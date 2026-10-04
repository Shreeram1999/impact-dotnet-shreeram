using System.ComponentModel.DataAnnotations;

namespace StudentApi.Dtos;

// Task 6.15 - what POST api/auth/login accepts and returns.
public class LoginRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record LoginResponseDto(string Token, DateTime ExpiresAtUtc, string Username, string Role);

public record CurrentUserDto(string Username, string Role, string? DisplayName);

// Task 8.5 - the registration form's fields. These attributes are the
// server's copy of the rules the React form checks inline. The client-side
// checks are a convenience for the user; these are the ones that count.
public class RegisterRequestDto
{
    public const string PasswordRule = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,100}$";

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateOnly? DateOfBirth { get; set; }

    // Which kind of account this is - it becomes the login's role.
    [Required]
    [RegularExpression("^(Student|Teacher)$", ErrorMessage = "Designation must be Student or Teacher.")]
    public string Designation { get; set; } = string.Empty;

    // Becomes the username (dbo.Users.Username is NVARCHAR(100)).
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(PasswordRule, ErrorMessage = "Password needs 8+ characters with upper case, lower case and a digit.")]
    public string Password { get; set; } = string.Empty;
}

public record RegisterResponseDto(string Username, string Role, string DisplayName);
