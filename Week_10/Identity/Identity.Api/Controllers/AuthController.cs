using Identity.Api.Dtos;
using Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.Shared;

namespace Identity.Api.Controllers;

// Reached through the gateway as /identity/api/auth/... (the gateway strips
// the /identity prefix).
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

    [HttpGet("me")]
    [Authorize]
    public ActionResult<CurrentUserDto> Me() => Ok(new CurrentUserDto(
        User.FindFirst(PortalClaims.Name)?.Value ?? string.Empty,
        User.FindFirst(PortalClaims.Role)?.Value ?? string.Empty,
        User.FindFirst(PortalClaims.DisplayName)?.Value));
}
