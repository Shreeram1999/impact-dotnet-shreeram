using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Auth;

// Task 6.15 - reuses the generic InMemoryRepository<T> for storage and adds
// the one lookup a login needs. Seeded with one account per role so the
// role check can be demonstrated straight away (credentials are listed in
// README.md - they're demo accounts for a training project, not secrets).
// Passwords are hashed at startup, so even this seed data only ever holds
// PBKDF2 hashes in memory.
public class InMemoryUserStore : IUserStore
{
    private readonly InMemoryRepository<User> users = new();

    public InMemoryUserStore(IPasswordHasher hasher)
    {
        Add(new User { Username = "teacher1", PasswordHash = hasher.Hash("Teacher@123"), Role = Roles.Teacher });
        Add(new User { Username = "student1", PasswordHash = hasher.Hash("Student@123"), Role = Roles.Student });
    }

    // Usernames are case-insensitive ("Teacher1" logs in as "teacher1").
    public User? FindByUsername(string username) =>
        users.GetAll().FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

    public void Add(User user)
    {
        if (FindByUsername(user.Username) is not null)
            throw new InvalidOperationException($"Username '{user.Username}' already exists.");

        users.Add(user);
    }
}
