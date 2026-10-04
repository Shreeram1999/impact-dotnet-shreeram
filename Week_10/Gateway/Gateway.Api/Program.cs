using Gateway.Api;
using StudentPortal.Shared;

// Task 10.5 - the API gateway (YARP). The React client talks ONLY to this
// process. Routes (see appsettings.json "ReverseProxy"):
//   /identity/{**}  -> Identity   (anonymous: login/register must work logged out)
//   /academics/{**} -> Academics  (JWT required)
//   /reporting/{**} -> Reporting  (JWT required)
// The /service prefix is stripped, so /academics/api/students reaches
// Academics as /api/students.
//
// The JWT is validated HERE, once, with the same shared rules every service
// uses. A missing, expired or tampered token is rejected with 401 at the
// edge and never reaches a service.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPortalJwtAuthentication(builder.Configuration);

// CORS lives at the edge now: the services behind the gateway are never
// called from a browser directly.
builder.Services.AddCors(options => options.AddPolicy(GatewayDefaults.CorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .WithHeaders("Authorization", "Content-Type")
    .WithExposedHeaders("Location")));

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(ClaimForwarding.Apply);

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { service = "gateway", status = "ok" }));
app.MapReverseProxy();

app.Run();

public static class GatewayDefaults
{
    public const string CorsPolicy = "SpaClient";
}

public partial class Program
{
}
