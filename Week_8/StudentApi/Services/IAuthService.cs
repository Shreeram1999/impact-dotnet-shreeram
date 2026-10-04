using StudentApi.Auth;
using StudentApi.Models;

namespace StudentApi.Services;

public record LoginResult(bool Succeeded, IssuedToken? Token, User? User)
{
    public static LoginResult Failed { get; } = new(false, null, null);
}

// Week 8 - each outcome maps to exactly one status code in AuthController:
// Success 201, UsernameTaken 409, TeacherRegistrationDisabled 403,
// InvalidDateOfBirth 400.
public enum RegistrationOutcome
{
    Success,
    UsernameTaken,
    TeacherRegistrationDisabled,
    InvalidDateOfBirth
}

public record RegistrationResult(RegistrationOutcome Outcome, User? User = null);

public record RegistrationRequest(string Name, DateOnly DateOfBirth, string Designation, string Email, string Password);

// Task 6.15 - the login rule (find user, verify hash, issue token) lives in
// a Service like every other rule since Week 4, so AuthController stays as
// thin as StudentsController. Week 8 adds Register next to it.
public interface IAuthService
{
    LoginResult Login(string username, string password);
    RegistrationResult Register(RegistrationRequest request);
}
