using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using StudentApi.Auth;
using StudentApi.Models;

namespace StudentApi.Tests.Auth;

public class JwtTokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "StudentApi",
        Audience = "StudentPortal",
        Key = "unit-test-signing-key-0123456789-abcdefghijklmnop",
        ExpiryMinutes = 30
    };

    private static readonly DateTimeOffset FixedNow = new(2026, 9, 1, 9, 0, 0, TimeSpan.Zero);

    private static JwtTokenService CreateService()
    {
        var clock = new Mock<TimeProvider>();
        clock.Setup(c => c.GetUtcNow()).Returns(FixedNow);
        return new JwtTokenService(Microsoft.Extensions.Options.Options.Create(Options), clock.Object);
    }

    private static TokenValidationParameters ValidationParameters(string key) => new()
    {
        ValidIssuer = Options.Issuer,
        ValidAudience = Options.Audience,
        IssuerSigningKey = JwtTokenService.SigningKey(new JwtOptions { Key = key }),
        ValidateLifetime = false
    };

    [Fact]
    public void CreateToken_CarriesTheUsernameAndRoleClaims()
    {
        var issued = CreateService().CreateToken(new User { Id = 7, Username = "teacher1", Role = Roles.Teacher });

        var token = new JsonWebToken(issued.Token);
        Assert.Equal("teacher1", token.GetClaim("sub").Value);
        Assert.Equal("Teacher", token.GetClaim("role").Value);
        Assert.Equal("7", token.GetClaim("uid").Value);
    }

    [Fact]
    public void CreateToken_ExpiresAfterTheConfiguredMinutes()
    {
        var issued = CreateService().CreateToken(new User { Username = "u", Role = Roles.Student });

        Assert.Equal(FixedNow.UtcDateTime.AddMinutes(30), issued.ExpiresAtUtc);
        Assert.Equal(FixedNow.UtcDateTime.AddMinutes(30), new JsonWebToken(issued.Token).ValidTo);
    }

    [Fact]
    public async Task CreateToken_ValidatesWithTheSameKey()
    {
        var issued = CreateService().CreateToken(new User { Username = "u", Role = Roles.Student });

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(issued.Token, ValidationParameters(Options.Key));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateToken_FailsValidationWithADifferentKey()
    {
        var issued = CreateService().CreateToken(new User { Username = "u", Role = Roles.Student });

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(issued.Token,
            ValidationParameters("a-completely-different-key-0123456789-abcdefgh"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void JwtOptions_DefaultExpiry_IsOneHour()
    {
        Assert.Equal(60, new JwtOptions().ExpiryMinutes);
        Assert.Equal("Jwt", JwtOptions.SectionName);
    }
}
