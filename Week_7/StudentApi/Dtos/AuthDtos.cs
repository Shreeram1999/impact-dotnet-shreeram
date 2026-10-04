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

public record CurrentUserDto(string Username, string Role);
