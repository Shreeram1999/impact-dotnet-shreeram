using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApi.Auth;
using StudentApi.Dtos;
using StudentApi.Services;

namespace StudentApi.Controllers;

// Task 6.15 - authentication (who are you?) happens here once, at login.
// Every later request proves identity with the token instead of resending
// the password, and authorization (what may you do?) is enforced per
// endpoint by [Authorize(Roles = ...)] - on the server, never by the UI.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly ILogger<AuthController> logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    // POST api/auth/login -> 200 + token, or 401 (same response for an
    // unknown user and a wrong password).
    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponseDto> Login(LoginRequestDto request)
    {
        var result = authService.Login(request.Username, request.Password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed login for {Username}.", request.Username);
            return Unauthorized();
        }

        return Ok(new LoginResponseDto(result.Token!.Token, result.Token.ExpiresAtUtc, result.User!.Username, result.User.Role));
    }

    // GET api/auth/me -> who the presented token says you are (401 without
    // a valid token). Week 8's React client reads the role from here.
    [HttpGet("me")]
    [Authorize]
    public ActionResult<CurrentUserDto> Me()
    {
        return Ok(new CurrentUserDto(
            User.FindFirst(JwtTokenService.NameClaim)?.Value ?? string.Empty,
            User.FindFirst(JwtTokenService.RoleClaim)?.Value ?? string.Empty));
    }
}
