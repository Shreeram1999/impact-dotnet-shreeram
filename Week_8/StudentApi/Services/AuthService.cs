using Microsoft.Extensions.Options;
using StudentApi.Auth;
using StudentApi.Models;

namespace StudentApi.Services;

// Task 6.15 - an unknown username and a wrong password produce the SAME
// failed result (and the same 401 from the controller), so the login
// endpoint can't be used to discover which usernames exist.
//
// Task 8.5 - Register creates the login account behind the React
// registration form: email becomes the username, Designation becomes the
// role, the password is stored only as a PBKDF2 hash.
public class AuthService : IAuthService
{
    private const int MinimumAge = 5;

    private readonly IUserStore userStore;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenService tokenService;
    private readonly RegistrationOptions registrationOptions;
    private readonly TimeProvider timeProvider;

    public AuthService(
        IUserStore userStore,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<RegistrationOptions> registrationOptions,
        TimeProvider timeProvider)
    {
        this.userStore = userStore;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
        this.registrationOptions = registrationOptions.Value;
        this.timeProvider = timeProvider;
    }

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

        // A date of birth can't be in the future, and anyone younger than
        // the youngest student age (Task 5.6's Range(5, 100)) is a typo.
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
            // Lost a race with a simultaneous registration of the same email:
            // the store's uniqueness check (or the database's unique index)
            // caught it.
            return new RegistrationResult(RegistrationOutcome.UsernameTaken);
        }

        return new RegistrationResult(RegistrationOutcome.Success, user);
    }
}
