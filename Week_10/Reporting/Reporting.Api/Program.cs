using Microsoft.EntityFrameworkCore;
using Reporting.Api.Data;
using Reporting.Api.Services;
using StudentPortal.Shared;

// Task 10.4 - the Reporting service: read-mostly endpoints over a database
// someone else owns, through a context SCAFFOLDED from it (EF Core
// Database First). No migrations here, ever - see Data/README.md.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read-only workload: no change tracking by default.
builder.Services.AddDbContext<ReportingDbContext>((sp, options) => options
    .UseSqlServer(sp.GetRequiredService<IConfiguration>().GetConnectionString("ReportingDb"))
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddPortalJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "reporting", status = "ok" })).AllowAnonymous();

app.Run();

public partial class Program
{
}
