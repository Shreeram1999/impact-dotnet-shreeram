using System.Security.Cryptography;

namespace SecureFileVault.Streaming;

// Task 6.12 - "generate it first": writes a file of random bytes 1 MB at a
// time, so creating a 100 MB+ test file is itself streamed rather than
// building a 100 MB array in memory first.
public static class LargeFileGenerator
{
    public const int OneMegabyte = 1024 * 1024;

    public static long Generate(string path, int sizeInMegabytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeInMegabytes);

        var buffer = new byte[OneMegabyte];
        using var output = new FileStream(path, FileMode.Create, FileAccess.Write);
        for (var i = 0; i < sizeInMegabytes; i++)
        {
            RandomNumberGenerator.Fill(buffer);
            output.Write(buffer);
        }

        return output.Length;
    }
}
