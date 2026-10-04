using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StudentApi.Models;

namespace StudentApi.Auth;

// Task 6.15 - a minimal JWT: who you are (sub), what you may do (role), and
// when it stops working (exp), signed with HMAC-SHA256 so the server can
// tell later that nobody edited the claims. Note what the token is NOT: it
// isn't encrypted - anyone holding it can Base64-decode and read the claims
// (Task 6.2's encoding vs encryption). That's why it carries no password and
// no personal data, only identifiers.
public class JwtTokenService : ITokenService
{
    public const string RoleClaim = "role";
    public const string NameClaim = "sub";

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
                new Claim(NameClaim, user.Username),
                new Claim(RoleClaim, user.Role),
                new Claim("uid", user.Id.ToString())
            ]),
            SigningCredentials = new SigningCredentials(SigningKey(options), SecurityAlgorithms.HmacSha256)
        };

        return new IssuedToken(new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }

    public static SymmetricSecurityKey SigningKey(JwtOptions options) => new(Encoding.UTF8.GetBytes(options.Key));
}
