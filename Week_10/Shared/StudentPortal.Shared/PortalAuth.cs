using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace StudentPortal.Shared;

public static class Roles
{
    public const string Teacher = "Teacher";
    public const string Student = "Student";
}

// Claim names exactly as Identity issues them (no WS-* URI remapping).
public static class PortalClaims
{
    public const string Name = "sub";
    public const string Role = "role";
    public const string DisplayName = "name";
    public const string UserId = "uid";
}

// Bound from the "Jwt" section in every service. Identity SIGNS with Key;
// the gateway and the other services only VERIFY with it. (A production
// system would sign with a private key - RS256 - so verifiers never hold a
// secret that could also mint tokens; HS256 keeps this course's setup to
// one shared value, injected as the Jwt__Key environment variable.)
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Issuer { get; set; } = string.Empty;
    [Required] public string Audience { get; set; } = string.Empty;
    [Required, MinLength(32)] public string Key { get; set; } = string.Empty;
    [Range(1, 1440)] public int ExpiryMinutes { get; set; } = 60;

    public SymmetricSecurityKey SigningKey() => new(Encoding.UTF8.GetBytes(Key));
}

public static class PortalAuthExtensions
{
    // Week 6's JwtBearer setup, moved here so Identity, Academics, Reporting
    // and the Gateway all validate tokens identically - Task 10.2's "Identity
    // issues a JWT the other services accept".
    public static IServiceCollection AddPortalJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = ValidationParameters(jwt.Value);
            });
        services.AddAuthorization();
        return services;
    }

    public static TokenValidationParameters ValidationParameters(JwtOptions jwt) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,
        ValidateAudience = true,
        ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = jwt.SigningKey(),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = PortalClaims.Name,
        RoleClaimType = PortalClaims.Role
    };
}
