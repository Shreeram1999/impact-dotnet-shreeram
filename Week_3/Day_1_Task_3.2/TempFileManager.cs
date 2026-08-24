// Task 3.2 - IDisposable, `using`, and finalizers.
//
// Some resources (open files, network connections, database connections)
// are NOT managed by .NET's garbage collector the way normal objects are -
// the garbage collector only knows how to clean up MEMORY. A file handle
// held open by the operating system needs to be closed explicitly, or it
// stays open/locked even after your C# object is no longer used. The
// `IDisposable` interface is .NET's standard way of saying "this class
// holds onto something that needs explicit cleanup - call Dispose() when
// you're done with it".
public class TempFileManager : IDisposable
{
    public string FilePath { get; }

    // Tracks whether Dispose() has already run, so calling it twice (or
    // calling it and then having the finalizer ALSO try to run) doesn't
    // try to delete an already-deleted file.
    private bool disposed;

    public TempFileManager()
    {
        // Path.GetTempFileName() asks the operating system for a brand-new,
        // guaranteed-unique temp file and creates it immediately - so as
        // soon as this constructor finishes, the file already exists on disk.
        FilePath = Path.GetTempFileName();
        Console.WriteLine($"Created temp file: {FilePath}");
    }

    // This is a "finalizer" (sometimes called a destructor, written with a
    // `~` prefix). The garbage collector calls it automatically - NOT you -
    // sometime after the object becomes unreachable, if Dispose() was
    // never called. It exists purely as a SAFETY NET: if some caller
    // forgets to call Dispose() (or forgets to wrap the object in a
    // `using`), the finalizer gives us one last chance to clean up the
    // temp file instead of leaving it on disk forever.
    ~TempFileManager()
    {
        DeleteFileIfPresent();
    }

    // The proper, deterministic way to clean up: called explicitly by the
    // `using` statement (see Program.cs) as soon as the block ends, rather
    // than waiting for the unpredictable timing of the garbage collector.
    public void Dispose()
    {
        DeleteFileIfPresent();

        // GC.SuppressFinalize(this) tells the garbage collector "you don't
        // need to bother calling the finalizer for this object anymore -
        // cleanup already happened here in Dispose()". Without this call,
        // the GC would still schedule the finalizer to run later even
        // though there's nothing left to clean up, which wastes a bit of
        // work. So: the finalizer is the SAFETY NET for when Dispose() is
        // forgotten, and SuppressFinalize is how Dispose() tells the GC
        // "the safety net isn't needed this time, I already handled it".
        GC.SuppressFinalize(this);
    }

    private void DeleteFileIfPresent()
    {
        if (disposed)
            return;

        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            Console.WriteLine($"Deleted temp file: {FilePath}");
        }

        disposed = true;
    }
}
