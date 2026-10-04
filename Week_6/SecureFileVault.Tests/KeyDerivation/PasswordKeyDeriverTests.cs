using SecureFileVault.KeyDerivation;

namespace SecureFileVault.Tests.KeyDerivation;

public class PasswordKeyDeriverTests
{
    [Fact]
    public void SamePasswordAndSalt_AlwaysYieldTheSameKey()
    {
        var salt = PasswordKeyDeriver.GenerateSalt();

        Assert.Equal(PasswordKeyDeriver.DeriveKey("pa55word", salt), PasswordKeyDeriver.DeriveKey("pa55word", salt));
    }

    [Fact]
    public void DifferentSalt_YieldsADifferentKey()
    {
        Assert.NotEqual(
            PasswordKeyDeriver.DeriveKey("pa55word", PasswordKeyDeriver.GenerateSalt()),
            PasswordKeyDeriver.DeriveKey("pa55word", PasswordKeyDeriver.GenerateSalt()));
    }

    [Fact]
    public void DifferentPassword_YieldsADifferentKey()
    {
        var salt = PasswordKeyDeriver.GenerateSalt();

        Assert.NotEqual(PasswordKeyDeriver.DeriveKey("pa55word", salt), PasswordKeyDeriver.DeriveKey("pa55worD", salt));
    }

    [Fact]
    public void DifferentIterationCount_YieldsADifferentKey()
    {
        var salt = PasswordKeyDeriver.GenerateSalt();

        Assert.NotEqual(PasswordKeyDeriver.DeriveKey("pa55word", salt, 100_000), PasswordKeyDeriver.DeriveKey("pa55word", salt, 100_001));
    }

    [Fact]
    public void DerivedKey_Is32Bytes_AndSaltIs16Bytes()
    {
        var salt = PasswordKeyDeriver.GenerateSalt();

        Assert.Equal(16, salt.Length);
        Assert.Equal(32, PasswordKeyDeriver.DeriveKey("pa55word", salt).Length);
    }

    [Fact]
    public void ShortSalt_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => PasswordKeyDeriver.DeriveKey("pa55word", new byte[8]));
    }

    [Fact]
    public void TooFewIterations_AreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PasswordKeyDeriver.DeriveKey("pa55word", PasswordKeyDeriver.GenerateSalt(), 1_000));
    }

    [Fact]
    public void EmptyPassword_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => PasswordKeyDeriver.DeriveKey("", PasswordKeyDeriver.GenerateSalt()));
    }
}
