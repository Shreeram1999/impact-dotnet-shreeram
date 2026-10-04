using Identity.Api.Auth;
using Identity.Api.Data;
using Identity.Api.Models;
using Microsoft.Extensions.Options;
using StudentPortal.Shared;

namespace Identity.Api.Services;

public record LoginResult(bool Succeeded, IssuedToken? Token, User? User)
{
    public static LoginResult Failed { get; } = new(false, null, null);
}

public enum RegistrationOutcome
{
    Success,
    UsernameTaken,
    TeacherRegistrationDisabled,
    InvalidDateOfBirth
}

public record RegistrationResult(RegistrationOutcome Outcome, User? User = null);

public record RegistrationRequest(string Name, DateOnly DateOfBirth, string Designation, string Email, string Password);

public class RegistrationOptions
{
    public const string SectionName = "Registration";

    public bool AllowTeacherSelfRegistration { get; set; }
}

public interface IAuthService
{
    LoginResult Login(string username, string password);
    RegistrationResult Register(RegistrationRequest request);
}

// Weeks 6-8's login + registration rules, now the whole job of a service.
public class AuthService : IAuthService
{
    private const int MinimumAge = 5;

    private readonly IUserStore userStore;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenService tokenService;
    private readonly RegistrationOptions registrationOptions;
    private readonly TimeProvider timeProvider;

    public AuthService(IUserStore userStore, IPasswordHasher passwordHasher, ITokenService tokenService,
        IOptions<RegistrationOptions> registrationOptions, TimeProvider timeProvider)
    {
        this.userStore = userStore;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
        this.registrationOptions = registrationOptions.Value;
        this.timeProvider = timeProvider;
    }

    // Unknown user and wrong password fail identically (no username probing).
    public LoginResult Login(string username, string password)
    {
        var user = userStore.FindByUsername(username);
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
            return LoginResult.Failed;

        return new LoginResult(true, tokenService.CreateToken(user), user);
    }

    public RegistrationResult Register(RegistrationRequest request)
    {
        if (request.Designation == Roles.Teacher && !registrationOptions.AllowTeacherSelfRegistration)
            return new RegistrationResult(RegistrationOutcome.TeacherRegistrationDisabled);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (request.DateOfBirth > today.AddYears(-MinimumAge))
            return new RegistrationResult(RegistrationOutcome.InvalidDateOfBirth);

        var username = request.Email.Trim().ToLowerInvariant();
        if (userStore.FindByUsername(username) is not null)
            return new RegistrationResult(RegistrationOutcome.UsernameTaken);

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Designation,
            DisplayName = request.Name.Trim(),
            DateOfBirth = request.DateOfBirth
        };

        try
        {
            userStore.Add(user);
        }
        catch (InvalidOperationException)
        {
            return new RegistrationResult(RegistrationOutcome.UsernameTaken);
        }

        return new RegistrationResult(RegistrationOutcome.Success, user);
    }
}
