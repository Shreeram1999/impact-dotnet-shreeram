using StudentApi.Models;

namespace StudentApi.Auth;

// Task 6.15 - where login accounts live. In-memory this week; Week 7
// (Task 7.5) swaps in an ADO.NET implementation over SQL Server behind this
// exact interface, so AuthService and AuthController won't change.
public interface IUserStore
{
    User? FindByUsername(string username);
    void Add(User user);
}
