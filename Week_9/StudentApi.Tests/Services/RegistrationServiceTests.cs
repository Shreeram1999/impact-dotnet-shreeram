using Microsoft.Extensions.Options;
using Moq;
using StudentApi.Auth;
using StudentApi.Models;
using StudentApi.Services;

namespace StudentApi.Tests.Services;

// Task 8.5 - AuthService.Register with every dependency mocked. "Today" is
// pinned so the date-of-birth rule is deterministic.
public class RegistrationServiceTests
{
    private static readonly DateTimeOffset Today = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IUserStore> store = new();
    private readonly Mock<IPasswordHasher> hasher = new();

    private AuthService CreateService(bool allowTeachers = false)
    {
        var clock = new Mock<TimeProvider>();
        clock.Setup(c => c.GetUtcNow()).Returns(Today);
        hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("pbkdf2-hash");
        return new AuthService(store.Object, hasher.Object, Mock.Of<ITokenService>(),
            Options.Create(new RegistrationOptions { AllowTeacherSelfRegistration = allowTeachers }), clock.Object);
    }

    private static RegistrationRequest Request(string designation = Roles.Student, DateOnly? dob = null) =>
        new("  Asha Kumar ", dob ?? new DateOnly(2005, 5, 20), designation, " Asha@Example.com ", "Passw0rd!");

    [Fact]
    public void Student_IsRegistered_WithNormalisedEmailAndHashedPassword()
    {
        User? saved = null;
        store.Setup(s => s.Add(It.IsAny<User>())).Callback<User>(u => saved = u);

        var result = CreateService().Register(Request());

        Assert.Equal(RegistrationOutcome.Success, result.Outcome);
        Assert.Equal("asha@example.com", saved!.Username);
        Assert.Equal("pbkdf2-hash", saved.PasswordHash);
        Assert.Equal(Roles.Student, saved.Role);
        Assert.Equal("Asha Kumar", saved.DisplayName);
        Assert.Equal(new DateOnly(2005, 5, 20), saved.DateOfBirth);
        hasher.Verify(h => h.Hash("Passw0rd!"), Times.Once);
    }

    [Fact]
    public void Teacher_IsRejected_WhenSelfRegistrationIsDisabled()
    {
        var result = CreateService(allowTeachers: false).Register(Request(Roles.Teacher));

        Assert.Equal(RegistrationOutcome.TeacherRegistrationDisabled, result.Outcome);
        store.Verify(s => s.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Teacher_IsRegistered_WhenSelfRegistrationIsEnabled()
    {
        var result = CreateService(allowTeachers: true).Register(Request(Roles.Teacher));

        Assert.Equal(RegistrationOutcome.Success, result.Outcome);
        Assert.Equal(Roles.Teacher, result.User!.Role);
    }

    [Fact]
    public void ExistingEmail_IsUsernameTaken()
    {
        store.Setup(s => s.FindByUsername("asha@example.com")).Returns(new User { Username = "asha@example.com" });

        var result = CreateService().Register(Request());

        Assert.Equal(RegistrationOutcome.UsernameTaken, result.Outcome);
        store.Verify(s => s.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void StoreRejectingADuplicate_DuringTheInsert_IsAlsoUsernameTaken()
    {
        store.Setup(s => s.Add(It.IsAny<User>())).Throws(new InvalidOperationException("duplicate"));

        Assert.Equal(RegistrationOutcome.UsernameTaken, CreateService().Register(Request()).Outcome);
    }

    [Theory]
    [InlineData(2030, 1, 1)]   // future
    [InlineData(2022, 10, 2)]  // just under 5 years old
    public void ImplausibleDateOfBirth_IsInvalid(int year, int month, int day)
    {
        var result = CreateService().Register(Request(dob: new DateOnly(year, month, day)));

        Assert.Equal(RegistrationOutcome.InvalidDateOfBirth, result.Outcome);
    }

    [Fact]
    public void ExactlyFiveYearsOld_IsAccepted()
    {
        Assert.Equal(RegistrationOutcome.Success, CreateService().Register(Request(dob: new DateOnly(2021, 10, 1))).Outcome);
    }
}
