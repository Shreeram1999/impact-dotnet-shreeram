using System.Security.Claims;
using Identity.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StudentPortal.Shared;

namespace Identity.Api.Auth;

public record IssuedToken(string Token, DateTime ExpiresAtUtc);

public interface ITokenService
{
    IssuedToken CreateToken(User user);
}

// Task 10.2 - Identity is now the ONLY service that issues tokens. The claim
// names and the validation rules every other service applies live in
// StudentPortal.Shared, so what Identity signs is exactly what they accept.
public class JwtTokenService : ITokenService
{
    private readonly JwtOptions options;
    private readonly TimeProvider timeProvider;

    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        this.options = options.Value;
        this.timeProvider = timeProvider;
    }

    public IssuedToken CreateToken(User user)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(options.ExpiryMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            Subject = new ClaimsIdentity(
            [
                new Claim(PortalClaims.Name, user.Username),
                new Claim(PortalClaims.Role, user.Role),
                new Claim(PortalClaims.UserId, user.Id.ToString()),
                new Claim(PortalClaims.DisplayName, user.DisplayName ?? user.Username)
            ]),
            SigningCredentials = new SigningCredentials(options.SigningKey(), SecurityAlgorithms.HmacSha256)
        };

        return new IssuedToken(new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}
