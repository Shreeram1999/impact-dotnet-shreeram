using Identity.Api.Auth;
using Identity.Api.Data;
using StudentPortal.Shared;

namespace Identity.Tests.Auth;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher hasher = new();

    [Fact]
    public void Verify_AcceptsTheRightPassword()
    {
        var hash = hasher.Hash("Teacher@123");

        Assert.True(hasher.Verify("Teacher@123", hash));
    }

    [Theory]
    [InlineData("teacher@123")]
    [InlineData("Teacher@1234")]
    [InlineData("")]
    public void Verify_RejectsAWrongPassword(string attempt)
    {
        var hash = hasher.Hash("Teacher@123");

        Assert.False(hasher.Verify(attempt, hash));
    }

    [Fact]
    public void Hash_IsSalted_SoTheSamePasswordHashesDifferentlyEachTime()
    {
        Assert.NotEqual(hasher.Hash("same"), hasher.Hash("same"));
    }

    [Fact]
    public void Hash_NeverContainsThePassword_AndRecordsSchemeAndIterations()
    {
        var hash = hasher.Hash("Teacher@123");

        Assert.DoesNotContain("Teacher@123", hash);
        Assert.StartsWith("PBKDF2-SHA256$100000$", hash);
        Assert.Equal(4, hash.Split('$').Length);
    }

    [Theory]
    [InlineData("")]
    [InlineData("plain-text-password")]
    [InlineData("MD5$1$abc$def")]
    [InlineData("PBKDF2-SHA256$notanumber$AAAA$AAAA")]
    [InlineData("PBKDF2-SHA256$0$AAAA$AAAA")]
    [InlineData("PBKDF2-SHA256$100000$***$AAAA")]
    public void Verify_MalformedStoredHash_ReturnsFalseInsteadOfThrowing(string storedHash)
    {
        Assert.False(hasher.Verify("anything", storedHash));
    }

    [Fact]
    public void Hash_EmptyPassword_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => hasher.Hash(""));
    }
}
