namespace SecureFileVault.Tests.TestSupport;

// A throwaway folder per test, deleted on Dispose, so file-based tests never
// collide with each other when xUnit runs test classes in parallel.
public sealed class TempFolder : IDisposable
{
    public string Path { get; } = Directory.CreateTempSubdirectory("sfv-tests-").FullName;

    public string File(string name) => System.IO.Path.Combine(Path, name);

    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}
