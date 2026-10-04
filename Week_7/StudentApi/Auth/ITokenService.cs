using StudentApi.Models;

namespace StudentApi.Auth;

public record IssuedToken(string Token, DateTime ExpiresAtUtc);

// Task 6.15 - issues the JWT a successful login hands back.
public interface ITokenService
{
    IssuedToken CreateToken(User user);
}
