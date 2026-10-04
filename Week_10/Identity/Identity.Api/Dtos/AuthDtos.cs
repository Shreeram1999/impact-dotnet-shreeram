using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Dtos;

public class LoginRequestDto
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public record LoginResponseDto(string Token, DateTime ExpiresAtUtc, string Username, string Role);

public record CurrentUserDto(string Username, string Role, string? DisplayName);

public class RegisterRequestDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateOnly? DateOfBirth { get; set; }

    [Required, RegularExpression("^(Student|Teacher)$", ErrorMessage = "Designation must be Student or Teacher.")]
    public string Designation { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,100}$",
        ErrorMessage = "Password needs 8+ characters with upper case, lower case and a digit.")]
    public string Password { get; set; } = string.Empty;
}

public record RegisterResponseDto(string Username, string Role, string DisplayName);
