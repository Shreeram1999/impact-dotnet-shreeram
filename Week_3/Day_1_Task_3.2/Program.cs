string capturedPath;

// `using (...)` guarantees Dispose() is called automatically at the end of
// this block, EVEN IF an exception is thrown inside it - equivalent to
// wrapping everything in a try/finally where the finally calls Dispose().
// This is the standard, safe way to work with any IDisposable resource.
using (var manager = new TempFileManager())
{
    capturedPath = manager.FilePath;
    Console.WriteLine($"Inside the using block, file exists: {File.Exists(manager.FilePath)}");
}
// By the time execution reaches here, manager.Dispose() has already run.

Console.WriteLine($"After the using block, file exists: {File.Exists(capturedPath)}");
