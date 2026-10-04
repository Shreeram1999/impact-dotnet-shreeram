using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StudentApi.Auth;
using StudentApi.Data;
using StudentApi.Models;
using StudentApi.Services;
using StudentApi.Services.Grading;

// Week 8 - the Week 7 API (three data layers, JWT auth) prepared for the
// React client: registration, Teacher-only writes everywhere, and CORS.
// Pipeline order:
//   1. Swagger / Swagger UI
//   2. HTTPS Redirection
//   3. CORS            - NEW: answers the browser's preflight and adds the
//                        Access-Control-Allow-* headers for the SPA origin
//   4. Authentication  - reads the Bearer token, builds HttpContext.User
//   5. Authorization   - checks [Authorize(Roles = ...)] against that User
//   6. Controller endpoint routing
// CORS must run before authentication/authorization. A preflight OPTIONS
// request never carries the token, so if it reached [Authorize] first it
// would be rejected and the browser would block the real request.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger's Authorize button, so a token from POST api/auth/login can be
// pasted in and the Teacher-only endpoint exercised from the UI.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the token from POST /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Week 7 - the repositories and user store now come from whichever data
// layer "DataLayer:Provider" in appsettings.json selects (Task 7.11 - see
// Data/DataLayerRegistration.cs). Nothing below this line changed.
builder.Services.AddDataLayer(builder.Configuration);
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddSingleton<IGradeStrategyFactory, GradeStrategyFactory>();

// Task 6.15 - auth. The hasher and token service are stateless (Singleton);
// AuthService follows the other Services (Scoped). IUserStore is registered
// by AddDataLayer above (Task 7.5 - login now hits the database).
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<RegistrationOptions>()
    .Bind(builder.Configuration.GetSection(RegistrationOptions.SectionName));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

// Configured through options (not inline above) so the signing key is read
// from the bound JwtOptions when the first request arrives, rather than
// from builder.Configuration at startup.
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
    {
        // Keep claim names exactly as issued ("role", "sub") instead of
        // remapping them to long WS-* URIs.
        bearer.MapInboundClaims = false;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Value.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = JwtTokenService.SigningKey(jwt.Value),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtTokenService.NameClaim,
            RoleClaimType = JwtTokenService.RoleClaim
        };
    });
builder.Services.AddAuthorization();

// Task 8.9 - CORS. The SPA is served from a different origin
// (http://localhost:5173) than the API (http://localhost:5095), so the
// browser refuses to hand the SPA any API response unless the API says that
// origin is allowed. Tightened to exactly what the client uses: listed
// origins only (Cors:AllowedOrigins), the four verbs, and the two request
// headers it sends. No AllowAnyOrigin, and no credentials mode (the token
// travels in a header, not a cookie).
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicies.SpaClient, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .WithHeaders("Authorization", "Content-Type")
        .WithExposedHeaders("Location"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(CorsPolicies.SpaClient);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets WebApplicationFactory<Program> boot this app in-process for the
// integration tests (see Week 5 for the full explanation).
public partial class Program
{
}

public static class CorsPolicies
{
    public const string SpaClient = "SpaClient";
}
