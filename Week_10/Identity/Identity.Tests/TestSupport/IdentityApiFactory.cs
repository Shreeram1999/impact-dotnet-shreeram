using Identity.Api.Auth;
using Identity.Api.Data;
using Identity.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StudentPortal.Shared;

namespace Identity.Tests.TestSupport;

// Boots the real Identity pipeline with an in-memory IUserStore in place of
// SQL Server, seeded like 03_SeedData.sql.
public class IdentityApiFactory : WebApplicationFactory<Program>
{
    public InMemoryUserStore Users { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var hasher = new Pbkdf2PasswordHasher();
            Users.Add(new User { Username = "teacher1", PasswordHash = hasher.Hash("Teacher@123"), Role = Roles.Teacher, DisplayName = "Meera Iyer" });
            Users.Add(new User { Username = "student1", PasswordHash = hasher.Hash("Student@123"), Role = Roles.Student, DisplayName = "Asha Kumar" });
            services.AddSingleton<IUserStore>(Users);
        });
    }
}

public class InMemoryUserStore : IUserStore
{
    private readonly List<User> users = [];

    public User? FindByUsername(string username) =>
        users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

    public void Add(User user)
    {
        if (FindByUsername(user.Username) is not null)
            throw new InvalidOperationException("duplicate");
        user.Id = users.Count + 1;
        users.Add(user);
    }
}
