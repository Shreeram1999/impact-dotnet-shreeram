using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StudentApi.Services;
using System.Linq;

namespace StudentApi.Tests.TestSupport;

// Boots the real StudentApi app in-process (real Program.cs, real
// middleware pipeline, real [ApiController] validation) but swaps the real
// IStudentService registration for FakeStudentService, so these tests never
// touch StudentService or InMemoryRepository - just the HTTP/controller
// layer sitting on top of an in-memory service, per the Testing Focus.
//
// A new instance is created per test (see StudentsControllerTests), rather
// than shared via IClassFixture, specifically so each test starts from an
// empty student list instead of leaking state from whichever test ran
// before it.
public class StudentApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var studentServiceDescriptor = services.Single(d => d.ServiceType == typeof(IStudentService));
            services.Remove(studentServiceDescriptor);
            services.AddSingleton<IStudentService, FakeStudentService>();

            var teacherServiceDescriptor = services.Single(d => d.ServiceType == typeof(ITeacherService));
            services.Remove(teacherServiceDescriptor);
            services.AddSingleton<ITeacherService, FakeTeacherService>();
        });
    }
}
