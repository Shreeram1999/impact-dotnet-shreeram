using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentApi.Auth;
using StudentApi.Data.AdoNet;
using StudentApi.Data.EfCodeFirst;
using StudentApi.Data.EfDbFirst;
using StudentApi.Models;

namespace StudentApi.Data;

public enum DataLayerProvider
{
    InMemory,
    AdoNet,
    EfCodeFirst,
    EfDbFirst
}

public class DataLayerOptions
{
    public const string SectionName = "DataLayer";

    public DataLayerProvider Provider { get; set; } = DataLayerProvider.InMemory;
}

// Task 7.11 - ONE configuration value ("DataLayer:Provider") decides which
// implementation sits behind IRepository<Student>, IRepository<Teacher> and
// IUserStore. Controllers and Services only ever ask for those interfaces,
// so switching the whole data layer is a config change, not a code change:
//
//   InMemory    - Week 5/6 List<T> stores (no database; used by the tests)
//   AdoNet      - Data/AdoNet       over StudentPortal_Week9 (hand-written schema)
//   EfCodeFirst - Data/EfCodeFirst  over StudentPortal_Week9_EfCodeFirst (migrations)
//   EfDbFirst   - Data/EfDbFirst    scaffolded from StudentPortal_Week9
//
// The choice is made when a repository is first resolved (inside each
// factory lambda), not while Program.cs is registering services. That way
// configuration added later - e.g. by WebApplicationFactory in the tests -
// is still honoured.
public static class DataLayerRegistration
{
    public const string EfCodeFirstConnection = "StudentPortalEfCodeFirst";

    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DataLayerOptions>().Bind(configuration.GetSection(DataLayerOptions.SectionName));

        // In-memory stores are Singleton - their List<T> IS the database, so
        // it must outlive any one request.
        services.AddSingleton<InMemoryRepository<Student>>();
        services.AddSingleton<InMemoryRepository<Teacher>>();
        services.AddSingleton<InMemoryUserStore>();

        // ADO.NET: the connection factory only holds a string, so Singleton.
        // Repositories open and close their own connection per call.
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<AdoNetStudentRepository>();
        services.AddScoped<AdoNetTeacherRepository>();
        services.AddScoped<AdoNetUserStore>();

        // EF Core: one DbContext per request (Scoped). Connection strings are
        // looked up lazily, only if that provider is actually selected.
        services.AddDbContext<AppDbContext>((sp, options) =>
            options.UseSqlServer(sp.GetRequiredService<IConfiguration>().GetConnectionString(EfCodeFirstConnection)));
        services.AddScoped<EfStudentRepository>();
        services.AddScoped<EfTeacherRepository>();
        services.AddScoped<EfUserStore>();

        services.AddDbContext<StudentPortalDbFirstContext>((sp, options) =>
            options.UseSqlServer(sp.GetRequiredService<IConfiguration>().GetConnectionString(SqlConnectionFactory.ConnectionName)));
        services.AddScoped<DbFirstStudentRepository>();
        services.AddScoped<DbFirstTeacherRepository>();
        services.AddScoped<DbFirstUserStore>();

        // The three seams. Scoped, because the EF implementations hold a
        // Scoped DbContext; for InMemory the lambda just hands back the
        // shared Singleton.
        services.AddScoped<IRepository<Student>>(sp => Provider(sp) switch
        {
            DataLayerProvider.AdoNet => sp.GetRequiredService<AdoNetStudentRepository>(),
            DataLayerProvider.EfCodeFirst => sp.GetRequiredService<EfStudentRepository>(),
            DataLayerProvider.EfDbFirst => sp.GetRequiredService<DbFirstStudentRepository>(),
            _ => sp.GetRequiredService<InMemoryRepository<Student>>()
        });

        services.AddScoped<IRepository<Teacher>>(sp => Provider(sp) switch
        {
            DataLayerProvider.AdoNet => sp.GetRequiredService<AdoNetTeacherRepository>(),
            DataLayerProvider.EfCodeFirst => sp.GetRequiredService<EfTeacherRepository>(),
            DataLayerProvider.EfDbFirst => sp.GetRequiredService<DbFirstTeacherRepository>(),
            _ => sp.GetRequiredService<InMemoryRepository<Teacher>>()
        });

        services.AddScoped<IUserStore>(sp => Provider(sp) switch
        {
            DataLayerProvider.AdoNet => sp.GetRequiredService<AdoNetUserStore>(),
            DataLayerProvider.EfCodeFirst => sp.GetRequiredService<EfUserStore>(),
            DataLayerProvider.EfDbFirst => sp.GetRequiredService<DbFirstUserStore>(),
            _ => sp.GetRequiredService<InMemoryUserStore>()
        });

        return services;
    }

    private static DataLayerProvider Provider(IServiceProvider sp) =>
        sp.GetRequiredService<IOptions<DataLayerOptions>>().Value.Provider;
}
