using Moq;
using StudentApi.Auth;
using StudentApi.Models;
using StudentApi.Services;

namespace StudentApi.Tests.Services;

// The login path with every dependency mocked: the store, the hasher and
// the token service.
public class AuthServiceTests
{
    private static readonly User Teacher = new() { Id = 1, Username = "teacher1", PasswordHash = "stored-hash", Role = Roles.Teacher };

    private readonly Mock<IUserStore> store = new();
    private readonly Mock<IPasswordHasher> hasher = new();
    private readonly Mock<ITokenService> tokens = new();

    private AuthService CreateService() => new(store.Object, hasher.Object, tokens.Object,
        Microsoft.Extensions.Options.Options.Create(new RegistrationOptions()), TimeProvider.System);

    [Fact]
    public void Login_RightPassword_IssuesAToken()
    {
        store.Setup(s => s.FindByUsername("teacher1")).Returns(Teacher);
        hasher.Setup(h => h.Verify("Teacher@123", "stored-hash")).Returns(true);
        tokens.Setup(t => t.CreateToken(Teacher)).Returns(new IssuedToken("jwt", DateTime.UtcNow));

        var result = CreateService().Login("teacher1", "Teacher@123");

        Assert.True(result.Succeeded);
        Assert.Equal("jwt", result.Token!.Token);
        Assert.Same(Teacher, result.User);
    }

    [Fact]
    public void Login_WrongPassword_FailsWithoutIssuingAToken()
    {
        store.Setup(s => s.FindByUsername("teacher1")).Returns(Teacher);
        hasher.Setup(h => h.Verify("wrong", "stored-hash")).Returns(false);

        var result = CreateService().Login("teacher1", "wrong");

        Assert.False(result.Succeeded);
        Assert.Null(result.Token);
        tokens.Verify(t => t.CreateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Login_UnknownUser_FailsTheSameWay_AndNeverChecksAPassword()
    {
        store.Setup(s => s.FindByUsername("ghost")).Returns((User?)null);

        var result = CreateService().Login("ghost", "anything");

        Assert.Same(LoginResult.Failed, result);
        hasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
