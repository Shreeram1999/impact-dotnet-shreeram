using System.Security.Cryptography;
using SecureFileVault.FileHandling;
using SecureFileVault.Tests.TestSupport;

namespace SecureFileVault.Tests.FileHandling;

public class FileStreamBasicsTests
{
    [Fact]
    public void WriteThenAppend_ReadTextReturnsBothParts()
    {
        using var temp = new TempFolder();
        var path = temp.File("notes.txt");

        FileStreamBasics.WriteText(path, "first ");
        FileStreamBasics.AppendText(path, "second");

        Assert.Equal("first second", FileStreamBasics.ReadText(path));
    }

    [Fact]
    public void WriteText_OverwritesAnExistingFile()
    {
        using var temp = new TempFolder();
        var path = temp.File("notes.txt");

        FileStreamBasics.WriteText(path, "old content that is longer");
        FileStreamBasics.WriteText(path, "new");

        Assert.Equal("new", FileStreamBasics.ReadText(path));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(4096, 1)]
    [InlineData(4097, 2)]
    [InlineData(100_000, 25)]
    public void CopyInChunks_ReproducesTheFileByteForByte_In4KbReads(int size, int expectedChunks)
    {
        using var temp = new TempFolder();
        var source = temp.File("source.bin");
        var destination = temp.File("copy.bin");
        var data = RandomNumberGenerator.GetBytes(size);
        File.WriteAllBytes(source, data);

        var chunks = FileStreamBasics.CopyInChunks(source, destination);

        Assert.Equal(expectedChunks, chunks);
        Assert.Equal(data, File.ReadAllBytes(destination));
    }

    [Fact]
    public void StreamCopy_ProducesAnIdenticalFile()
    {
        using var temp = new TempFolder();
        var source = temp.File("source.bin");
        var destination = temp.File("copy.bin");
        var data = RandomNumberGenerator.GetBytes(10_000);
        File.WriteAllBytes(source, data);

        FileStreamBasics.StreamCopy(source, destination);

        Assert.Equal(data, File.ReadAllBytes(destination));
    }

    [Fact]
    public void ReadText_MissingFile_Throws()
    {
        using var temp = new TempFolder();

        Assert.Throws<FileNotFoundException>(() => FileStreamBasics.ReadText(temp.File("missing.txt")));
    }
}
