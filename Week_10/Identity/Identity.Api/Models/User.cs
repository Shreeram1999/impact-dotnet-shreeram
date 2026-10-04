namespace Identity.Api.Models;

// A login account. Only the PBKDF2 hash is ever stored (Week 6).
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
