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

    // Task 8.5 - POST api/auth/register -> 201, or 400 (DataAnnotations or
    // an impossible date of birth), 403 (Teacher self-registration is
    // disabled), 409 (email already registered). No token is issued here;
    // the client logs in afterwards like any other user.
    [HttpPost("register")]
    [AllowAnonymous]
    public ActionResult<RegisterResponseDto> Register(RegisterRequestDto request)
    {
        var result = authService.Register(new RegistrationRequest(
            request.Name, request.DateOfBirth!.Value, request.Designation, request.Email, request.Password));

        switch (result.Outcome)
        {
            case RegistrationOutcome.UsernameTaken:
                return Conflict("An account with that email already exists.");
            case RegistrationOutcome.TeacherRegistrationDisabled:
                return Problem(statusCode: StatusCodes.Status403Forbidden, title: "Teacher accounts can't be self-registered.");
            case RegistrationOutcome.InvalidDateOfBirth:
                ModelState.AddModelError(nameof(request.DateOfBirth), "Date of birth must be at least 5 years in the past.");
                return ValidationProblem(ModelState);
        }

        var user = result.User!;
        logger.LogInformation("Registered {Username} as {Role}.", user.Username, user.Role);
        return Created("/api/auth/me", new RegisterResponseDto(user.Username, user.Role, user.DisplayName ?? string.Empty));
    }

    // GET api/auth/me -> who the presented token says you are (401 without
    // a valid token). The React client reads the role from here or from the
    // token itself.
    [HttpGet("me")]
    [Authorize]
    public ActionResult<CurrentUserDto> Me()
    {
        return Ok(new CurrentUserDto(
            User.FindFirst(JwtTokenService.NameClaim)?.Value ?? string.Empty,
            User.FindFirst(JwtTokenService.RoleClaim)?.Value ?? string.Empty,
            User.FindFirst(JwtTokenService.DisplayNameClaim)?.Value));
    }
}
