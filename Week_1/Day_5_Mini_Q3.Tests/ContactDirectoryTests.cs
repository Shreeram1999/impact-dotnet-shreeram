public class ContactDirectoryTests
{
    private static ContactCard[] SampleContacts() => new ContactCard[]
    {
        new("Alice Johnson", "555-0101", "alice@example.com"),
        new("Bob Smith", "555-0102", "bob@example.com"),
        new("Charlie Davis", "555-0103", "charlie@example.com"),
    };

    [Fact]
    public void FindByName_ExactCaseMatch_ReturnsContact()
    {
        var result = ContactDirectory.FindByName(SampleContacts(), "Bob Smith");

        Assert.NotNull(result);
        Assert.Equal("555-0102", result.Value.Phone);
    }

    [Fact]
    public void FindByName_LowercaseQuery_MatchesDifferentlyCasedStoredName()
    {
        var result = ContactDirectory.FindByName(SampleContacts(), "charlie davis");

        Assert.NotNull(result);
        Assert.Equal("Charlie Davis", result.Value.Name);
        Assert.Equal("charlie@example.com", result.Value.Email);
    }

    [Fact]
    public void FindByName_UppercaseQuery_MatchesDifferentlyCasedStoredName()
    {
        var result = ContactDirectory.FindByName(SampleContacts(), "ALICE JOHNSON");

        Assert.NotNull(result);
        Assert.Equal("Alice Johnson", result.Value.Name);
    }

    [Fact]
    public void FindByName_MixedCaseQuery_StillMatches()
    {
        var result = ContactDirectory.FindByName(SampleContacts(), "aLiCe JoHnSoN");

        Assert.NotNull(result);
        Assert.Equal("Alice Johnson", result.Value.Name);
    }

    [Fact]
    public void FindByName_NoMatch_ReturnsNull()
    {
        var result = ContactDirectory.FindByName(SampleContacts(), "Nonexistent Person");

        Assert.Null(result);
    }

    [Fact]
    public void FindByName_EmptyArray_ReturnsNull()
    {
        var result = ContactDirectory.FindByName(Array.Empty<ContactCard>(), "Anyone");

        Assert.Null(result);
    }

    [Fact]
    public void FindByName_EmptyQuery_ReturnsNull()
    {
        var result = ContactDirectory.FindByName(SampleContacts(), "");

        Assert.Null(result);
    }

    [Fact]
    public void FindByName_PartialSubstring_DoesNotMatch()
    {
        // "Charlie" alone is a substring of "Charlie Davis" but not an exact
        // (case-insensitive) match, so this must NOT be found — guards
        // against an accidental switch to Contains()-style matching.
        var result = ContactDirectory.FindByName(SampleContacts(), "Charlie");

        Assert.Null(result);
    }
}
