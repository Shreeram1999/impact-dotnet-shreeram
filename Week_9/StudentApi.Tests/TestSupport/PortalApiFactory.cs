using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StudentApi.Tests.TestSupport;

// Unlike StudentApiFactory (which swaps the Services for in-memory fakes to
// test the HTTP layer alone), this boots the REAL Services on the in-memory
// data layer. Tests then replace just the piece they need (e.g. a
// repository that throws) with WithWebHostBuilder + ConfigureTestServices.
public class PortalApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("DataLayer:Provider", "InMemory");
    }
}
