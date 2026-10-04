using Identity.Api.Auth;
using Identity.Api.Data;
using Identity.Api.Services;
using StudentPortal.Shared;

// Task 10.2 - the Identity service: Week 6's minimal auth promoted into its
// own deployable, with its own database (IdentityDb) reached only through
// ADO.NET + stored procedures. It's the only service that knows passwords
// exist; everything else only ever sees the signed JWT it issues.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<RegistrationOptions>().Bind(builder.Configuration.GetSection(RegistrationOptions.SectionName));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IUserStore, AdoNetUserStore>();
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Identity also VALIDATES its own tokens, for GET /api/auth/me.
builder.Services.AddPortalJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "identity", status = "ok" })).AllowAnonymous();

app.Run();

public partial class Program
{
}
