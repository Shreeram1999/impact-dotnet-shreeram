using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StudentApi.Auth;
using StudentApi.Data;
using StudentApi.Models;
using StudentApi.Services;
using StudentApi.Services.Grading;

// Week 7 - the Week 6 API (Student CRUD + minimal JWT auth) on real
// persistence: ADO.NET, EF Code First or EF DB First, picked by one
// appsettings value. Pipeline order (unchanged from Week 6):
//   1. Swagger / Swagger UI
//   2. HTTPS Redirection
//   3. Authentication  - NEW: reads the Bearer token, builds HttpContext.User
//   4. Authorization   - checks [Authorize(Roles = ...)] against that User
//   5. Controller endpoint routing
// Authentication MUST come before Authorization: swap them and every request
// reaches the authorization check as an anonymous user, so a valid Teacher
// token would still get 401.

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets WebApplicationFactory<Program> boot this app in-process for the
// integration tests (see Week 5 for the full explanation).
public partial class Program
{
}
