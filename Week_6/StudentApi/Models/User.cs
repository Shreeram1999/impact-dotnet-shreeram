using StudentApi.Data;

namespace StudentApi.Models;

// Task 6.15 - a login account. There is deliberately no Password property:
// only the PBKDF2 hash is ever stored (see Auth/Pbkdf2PasswordHasher.cs), so
// even a full dump of the user store gives an attacker nothing to log in
// with directly.
public class User : IEntity
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
