using System.Text;
using SecureFileVault.Integrity;
using SecureFileVault.Tests.TestSupport;

namespace SecureFileVault.Tests.Integrity;

public class FileHasherTests
{
    private static MemoryStream Abc() => new(Encoding.ASCII.GetBytes("abc"));

    [Fact]
    public void Sha256_MatchesTheKnownTestVector()
    {
        Assert.Equal("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", Convert.ToHexString(FileHasher.Sha256(Abc())));
    }

    [Fact]
    public void Sha512_IsStableAcrossRuns_And64Bytes()
    {
        var first = FileHasher.Sha512(Abc());

        Assert.Equal(64, first.Length);
        Assert.Equal(first, FileHasher.Sha512(Abc()));
    }

    [Fact]
    public void Hmac_ChangesWhenTheKeyChanges_EvenThoughTheDataDidNot()
    {
        var withKeyOne = FileHasher.HmacSha256(Abc(), "key-one"u8.ToArray());
        var withKeyTwo = FileHasher.HmacSha256(Abc(), "key-two"u8.ToArray());

        Assert.NotEqual(withKeyOne, withKeyTwo);
        Assert.Equal(withKeyOne, FileHasher.HmacSha256(Abc(), "key-one"u8.ToArray()));
    }

    [Fact]
    public void FileVariants_MatchTheStreamVariants()
    {
        using var temp = new TempFolder();
        var path = temp.File("abc.txt");
        File.WriteAllText(path, "abc");
        var key = "k"u8.ToArray();

        Assert.Equal(FileHasher.Sha256(Abc()), FileHasher.Sha256File(path));
        Assert.Equal(FileHasher.Sha512(Abc()), FileHasher.Sha512File(path));
        Assert.Equal(FileHasher.HmacSha256(Abc(), key), FileHasher.HmacSha256File(path, key));
    }
}

public class DigestComparerTests
{
    [Fact]
    public void EqualDigests_AreEqual()
    {
        Assert.True(DigestComparer.AreEqual(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3 }));
    }

    [Fact]
    public void DigestsDifferingInTheLastByte_AreNotEqual()
    {
        Assert.False(DigestComparer.AreEqual(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 4 }));
    }

    [Fact]
    public void DigestsOfDifferentLength_AreNotEqual()
    {
        Assert.False(DigestComparer.AreEqual(new byte[] { 1, 2, 3 }, new byte[] { 1, 2 }));
    }
}
