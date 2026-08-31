using StudentApi.Data;
using StudentApi.Models;
using StudentApi.Services;
using StudentApi.Services.Grading;

// Task 5.1 - the DEFAULT middleware pipeline `dotnet new webapi -controllers`
// generated, before anything below was added (WeatherForecastController and
// its sample GET were deleted, and the empty app started cleanly on this
// pipeline first, per the Task 5.1 done-when):
//   1. (Development only) OpenAPI document mapping - app.MapOpenApi()
//   2. HTTPS Redirection            - app.UseHttpsRedirection()
//   3. Authorization                - app.UseAuthorization()
//      (no authentication middleware is registered - this project used
//      --auth None, and the whole API stays deliberately unsecured this week)
//   4. Controller endpoint routing  - app.MapControllers()
// Swagger's two middleware calls (UseSwagger/UseSwaggerUI, Task 5.8) are the
// only addition to that order below - everything else matches the template.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Task 5.3 - AddSingleton for the store (one shared List<T> for the whole
// app's lifetime - see InMemoryRepository's lock for why that's now safe
// under concurrent requests), AddScoped for the service (a fresh instance
// per HTTP request, which is what Scoped actually means once there IS a
// request boundary - contrast with Week 4's console app, where Scoped had
// no meaning and everything was Singleton instead).
builder.Services.AddSingleton<IRepository<Student>, InMemoryRepository<Student>>();
builder.Services.AddSingleton<IRepository<Teacher>, InMemoryRepository<Teacher>>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();

// Task 5.8 - stateless, so one shared instance for the app's lifetime is
// fine; nothing about picking a strategy depends on which request is asking.
builder.Services.AddSingleton<IGradeStrategyFactory, GradeStrategyFactory>();

var app = builder.Build();

// Left enabled outside Development on purpose - this API is unsecured by
// design this week, and Swagger is the easiest way to browse/exercise it.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets WebApplicationFactory<Program> (StudentApi.Tests) boot this app
// in-process for the controller/integration tests in Task 5.5/5.6's
// Testing Focus - required because Program.cs otherwise has no public type
// for the test project to reference, top-level statements being compiled
// into an internal Program class by default.
public partial class Program
{
}
