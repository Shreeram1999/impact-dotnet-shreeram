using System.ComponentModel.DataAnnotations;

namespace StudentApi.Auth;

// Task 6.15 - bound from the "Jwt" section of appsettings. The signing key
// lives in appsettings.Development.json for local runs only. Anywhere real
// it would come from user-secrets or the Jwt__Key environment variable,
// never from a committed file. ValidateOnStart (Program.cs) makes a missing
// or too-short key fail at startup, not on the first login.
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    // HMAC-SHA256 needs at least a 256-bit key.
    [Required]
    [MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 60;
}
