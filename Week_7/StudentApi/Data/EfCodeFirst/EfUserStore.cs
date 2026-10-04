using Microsoft.EntityFrameworkCore;
using StudentApi.Auth;
using StudentApi.Models;

namespace StudentApi.Data.EfCodeFirst;

// Task 7.6 - DbSet<User> behind the Week 6 IUserStore seam.
public class EfUserStore : IUserStore
{
    private readonly AppDbContext context;

    public EfUserStore(AppDbContext context)
    {
        this.context = context;
    }

    // Usernames are compared lower-cased so the lookup is case-insensitive
    // on every provider (SQL Server's default collation already is; SQLite's
    // isn't).
    public User? FindByUsername(string username)
    {
        var normalized = username.ToLower();
        return context.Users.AsNoTracking().FirstOrDefault(u => u.Username.ToLower() == normalized);
    }

    public void Add(User user)
    {
        if (FindByUsername(user.Username) is not null)
            throw new InvalidOperationException($"Username '{user.Username}' already exists.");

        context.Users.Add(user);
        context.SaveChanges();
    }
}
