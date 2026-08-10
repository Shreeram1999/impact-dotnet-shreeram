// xUnit basics for anyone new to .NET testing:
//   - `[Fact]` marks a method as a single test case with no parameters.
//   - `[Theory]` + `[InlineData(...)]` runs the SAME test method once per
//     data row, so we don't have to copy/paste near-identical tests.
//   - `Assert.Equal(expected, actual)` fails the test (with a helpful
//     message) if the two values aren't equal.
// This file covers ToTitleCase() for normal input, already-uppercase
// input (a trickier case - see the comment in Extensions.cs), a single
// character, and the empty-string edge case.
public class StringExtensionsTests
{
    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("HELLO WORLD", "Hello World")]
    [InlineData("a", "A")]
    public void ToTitleCase_VariousInputs_CapitalizesEachWord(string input, string expected)
    {
        Assert.Equal(expected, input.ToTitleCase());
    }

    [Fact]
    public void ToTitleCase_EmptyString_ReturnsEmptyString()
    {
        Assert.Equal("", "".ToTitleCase());
    }
}
