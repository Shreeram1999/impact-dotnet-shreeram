using SecureFileVault.Fundamentals;

namespace SecureFileVault.Tests.Fundamentals;

public class EncodingVsHashingTests
{
    [Theory]
    [InlineData("")]
    [InlineData("Student Portal")]
    [InlineData("unicode: café ✓")]
    public void Base64_RoundTrips(string text)
    {
        Assert.Equal(text, EncodingVsHashing.FromBase64(EncodingVsHashing.ToBase64(text)));
    }

    [Fact]
    public void Base64_IsNotSecret_AnyoneCanDecodeIt()
    {
        Assert.Equal("U2VjcmV0", EncodingVsHashing.ToBase64("Secret"));
    }

    [Fact]
    public void Sha256_MatchesTheKnownTestVector()
    {
        Assert.Equal("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", EncodingVsHashing.Sha256Hex("abc"));
    }

    [Fact]
    public void Sha256_IsFixedLengthWhateverTheInputSize()
    {
        Assert.Equal(64, EncodingVsHashing.Sha256Hex("a").Length);
        Assert.Equal(64, EncodingVsHashing.Sha256Hex(new string('a', 100_000)).Length);
    }
}
