using Academics.Api.Data;
using Academics.Api.Models;
using Academics.Api.Services;
using Academics.Api.Services.Grading;
using Microsoft.EntityFrameworkCore;
using StudentPortal.Shared;

// Task 10.3 - the Academics service: Students, Teachers, Courses and
// Enrollments on EF Core Code First, in its own database (AcademicsDb).
// It never sees a password: it trusts only the JWT that Identity signed,
// validated with the shared rules in StudentPortal.Shared.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AcademicsDbContext>((sp, options) =>
    options.UseSqlServer(sp.GetRequiredService<IConfiguration>().GetConnectionString("AcademicsDb")));
builder.Services.AddScoped<IRepository<Student>, EfRepository<Student>>();
builder.Services.AddScoped<IRepository<Teacher>, EfRepository<Teacher>>();
builder.Services.AddScoped<IRepository<Course>, EfRepository<Course>>();
builder.Services.AddScoped<IEnrollmentRepository, EfEnrollmentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddSingleton<IGradeStrategyFactory, GradeStrategyFactory>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddPortalJwtAuthentication(builder.Configuration);

var app = builder.Build();

// In docker-compose the service brings its own schema up to date on start
// ("Database:MigrateOnStartup": true). Locally, run `dotnet ef database
// update` instead, so schema changes stay an explicit step.
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AcademicsDbContext>().Database.Migrate();
}

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "academics", status = "ok" })).AllowAnonymous();

app.Run();

public partial class Program
{
}
