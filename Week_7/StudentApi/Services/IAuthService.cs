using StudentApi.Auth;
using StudentApi.Models;

namespace StudentApi.Services;

public record LoginResult(bool Succeeded, IssuedToken? Token, User? User)
{
    public static LoginResult Failed { get; } = new(false, null, null);
}

// Task 6.15 - the login rule (find user, verify hash, issue token) lives in
// a Service like every other rule since Week 4, so AuthController stays as
// thin as StudentsController.
public interface IAuthService
{
    LoginResult Login(string username, string password);
}
