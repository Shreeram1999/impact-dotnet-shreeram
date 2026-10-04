namespace StudentPortal.E2E.Infrastructure;

// Where the running portal is, and which browser drives it. Defaults match
// the local Week 10 stack (gateway :5100 + React :5173); override with
// environment variables (run-all-tests.ps1 sets them).
public static class E2ESettings
{
    public static string WebUrl { get; } = Env("E2E_WEB_URL", "http://localhost:5173").TrimEnd('/');
    public static string ApiUrl { get; } = Env("E2E_API_URL", "http://localhost:5100").TrimEnd('/');
    public static string Browser { get; } = Env("E2E_BROWSER", "chrome").ToLowerInvariant();
    public static bool Headless { get; } = !string.Equals(Env("E2E_HEADLESS", "true"), "false", StringComparison.OrdinalIgnoreCase);

    // Seeded demo accounts (Database/04_SeedData.sql, EF HasData).
    public const string TeacherUsername = "teacher1";
    public const string TeacherPassword = "Teacher@123";
    public const string StudentUsername = "student1";
    public const string StudentPassword = "Student@123";

    private static string Env(string name, string fallback) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : fallback;
}
