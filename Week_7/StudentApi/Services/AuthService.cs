using StudentApi.Auth;

namespace StudentApi.Services;

// Task 6.15 - an unknown username and a wrong password produce the SAME
// failed result (and the same 401 from the controller), so the login
// endpoint can't be used to discover which usernames exist.
public class AuthService : IAuthService
{
    private readonly IUserStore userStore;
    private readonly IPasswordHasher passwordHasher;
    private readonly ITokenService tokenService;

    public AuthService(IUserStore userStore, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        this.userStore = userStore;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
    }

    public LoginResult Login(string username, string password)
    {
        var user = userStore.FindByUsername(username);
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
            return LoginResult.Failed;

        return new LoginResult(true, tokenService.CreateToken(user), user);
    }
}
