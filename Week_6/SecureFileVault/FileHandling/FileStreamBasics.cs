namespace SecureFileVault.FileHandling;

// Task 6.1 - plain FileStream I/O, every stream inside a using so the OS
// handle is released the moment the block ends (even on an exception),
// rather than whenever the GC gets round to finalizing it - same idea as
// Week 3's IDisposable task (Day_1_Task_3.2), now applied to real files.
public static class FileStreamBasics
{
    public const int ChunkSize = 4 * 1024;

    public static void WriteText(string path, string text)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        writer.Write(text);
    }

    public static void AppendText(string path, string text)
    {
        using var stream = new FileStream(path, FileMode.Append, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        writer.Write(text);
    }

    public static string ReadText(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // Stream.CopyTo does its own buffered loop internally - the one-liner
    // version of what CopyInChunks below spells out by hand.
    public static void StreamCopy(string sourcePath, string destinationPath)
    {
        using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        using var destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
        source.CopyTo(destination);
    }

    // The point of Task 6.1: never File.ReadAllBytes a file of unknown size.
    // Only one 4 KB buffer is ever alive here, however large the file is -
    // memory use stays flat whether the input is 4 KB or 4 GB. Returns the
    // number of chunks read so the caller (and the tests) can see the file
    // really was consumed piecewise.
    public static int CopyInChunks(Stream source, Stream destination, int chunkSize = ChunkSize)
    {
        var buffer = new byte[chunkSize];
        var chunks = 0;
        int read;
        while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            destination.Write(buffer, 0, read);
            chunks++;
        }

        return chunks;
    }

    public static int CopyInChunks(string sourcePath, string destinationPath, int chunkSize = ChunkSize)
    {
        using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        using var destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
        return CopyInChunks(source, destination, chunkSize);
    }
}
