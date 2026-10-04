using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentApi.Auth;
using StudentApi.Data;
using StudentApi.Data.AdoNet;
using StudentApi.Data.EfCodeFirst;
using StudentApi.Data.EfDbFirst;
using StudentApi.Models;

namespace StudentApi.Tests.Data;

// Task 7.11 - "changing one config value switches the whole data layer".
// Resolving a repository never opens a connection (ADO.NET connects per
// call, EF on first query), so this needs no database.
public class DataLayerRegistrationTests
{
    private static IServiceProvider Build(string? provider)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:StudentPortalSql"] = "Server=example;Database=ado",
            ["ConnectionStrings:StudentPortalEfCodeFirst"] = "Server=example;Database=ef"
        };
        if (provider is not null)
            settings["DataLayer:Provider"] = provider;

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddDataLayer(configuration);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    [Theory]
    [InlineData("InMemory", typeof(InMemoryRepository<Student>), typeof(InMemoryRepository<Teacher>), typeof(InMemoryUserStore))]
    [InlineData("AdoNet", typeof(AdoNetStudentRepository), typeof(AdoNetTeacherRepository), typeof(AdoNetUserStore))]
    [InlineData("EfCodeFirst", typeof(EfStudentRepository), typeof(EfTeacherRepository), typeof(EfUserStore))]
    [InlineData("EfDbFirst", typeof(DbFirstStudentRepository), typeof(DbFirstTeacherRepository), typeof(DbFirstUserStore))]
    public void OneSetting_SelectsEveryImplementation(string provider, Type students, Type teachers, Type users)
    {
        using var scope = Build(provider).CreateScope();

        Assert.IsType(students, scope.ServiceProvider.GetRequiredService<IRepository<Student>>());
        Assert.IsType(teachers, scope.ServiceProvider.GetRequiredService<IRepository<Teacher>>());
        Assert.IsType(users, scope.ServiceProvider.GetRequiredService<IUserStore>());
    }

    [Fact]
    public void NoSetting_DefaultsToInMemory()
    {
        using var scope = Build(null).CreateScope();

        Assert.IsType<InMemoryRepository<Student>>(scope.ServiceProvider.GetRequiredService<IRepository<Student>>());
    }

    [Fact]
    public void InMemory_IsOneSharedStoreAcrossRequests()
    {
        var provider = Build("InMemory");
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        Assert.Same(
            first.ServiceProvider.GetRequiredService<IRepository<Student>>(),
            second.ServiceProvider.GetRequiredService<IRepository<Student>>());
    }

    [Fact]
    public void EfCodeFirst_GetsAFreshDbContextPerRequest()
    {
        var provider = Build("EfCodeFirst");
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        Assert.NotSame(
            first.ServiceProvider.GetRequiredService<AppDbContext>(),
            second.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}
