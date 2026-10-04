using Identity.Api.Auth;
using Identity.Api.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using StudentPortal.Shared;

namespace Identity.Tests.Auth;

// Task 10.2 - what Identity signs must be exactly what the other services
// accept, so these tests validate tokens with the SHARED validation rules
// (PortalAuthExtensions.ValidationParameters), not a hand-built copy.
public class JwtTokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "StudentPortal.Identity",
        Audience = "StudentPortal",
        Key = "unit-test-signing-key-0123456789-abcdefghijklmnop",
        ExpiryMinutes = 30
    };

    private static JwtTokenService CreateService(DateTimeOffset? now = null)
    {
        var clock = new Mock<TimeProvider>();
        clock.Setup(c => c.GetUtcNow()).Returns(now ?? DateTimeOffset.UtcNow);
        return new JwtTokenService(Microsoft.Extensions.Options.Options.Create(Options), clock.Object);
    }

    [Fact]
    public void Token_CarriesTheSharedClaimNames()
    {
        var issued = CreateService().CreateToken(new User { Id = 7, Username = "teacher1", Role = Roles.Teacher, DisplayName = "Meera Iyer" });

        var token = new JsonWebToken(issued.Token);
        Assert.Equal("teacher1", token.GetClaim(PortalClaims.Name).Value);
        Assert.Equal(Roles.Teacher, token.GetClaim(PortalClaims.Role).Value);
        Assert.Equal("7", token.GetClaim(PortalClaims.UserId).Value);
        Assert.Equal("Meera Iyer", token.GetClaim(PortalClaims.DisplayName).Value);
    }

    [Fact]
    public void Token_WithoutADisplayName_FallsBackToTheUsername()
    {
        var issued = CreateService().CreateToken(new User { Username = "u1", Role = Roles.Student });

        Assert.Equal("u1", new JsonWebToken(issued.Token).GetClaim(PortalClaims.DisplayName).Value);
    }

    [Fact]
    public async Task Token_PassesTheSameValidationEveryOtherServiceApplies()
    {
        var issued = CreateService().CreateToken(new User { Username = "u", Role = Roles.Student });

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(issued.Token, PortalAuthExtensions.ValidationParameters(Options));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Token_SignedWithAnotherKey_FailsSharedValidation()
    {
        var issued = CreateService().CreateToken(new User { Username = "u", Role = Roles.Student });
        var otherKey = new JwtOptions { Issuer = Options.Issuer, Audience = Options.Audience, Key = new string('z', 48) };

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(issued.Token, PortalAuthExtensions.ValidationParameters(otherKey));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ExpiredToken_FailsSharedValidation()
    {
        var issued = CreateService(DateTimeOffset.UtcNow.AddHours(-2)).CreateToken(new User { Username = "u", Role = Roles.Student });

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(issued.Token, PortalAuthExtensions.ValidationParameters(Options));

        Assert.False(result.IsValid);
        Assert.True(issued.ExpiresAtUtc < DateTime.UtcNow);
    }
}
