// These tests deliberately target the "edges" of ToWords()'s logic rather
// than just random middle values - that's called boundary testing:
//   - 0 and 19: the irregular, hand-written number names
//   - 20: the very first "regular" tens word
//   - 45: a tens+ones combo that needs the hyphen
//   - 100: an exact hundred with no remainder
//   - 305: a hundred WITH a remainder (exercises the recursive call)
//   - 999: the top of the supported range
// The last two tests check that going just outside the supported range
// (-1 and 1000) correctly throws, instead of returning garbage.
public class IntExtensionsTests
{
    [Theory]
    [InlineData(0, "Zero")]
    [InlineData(7, "Seven")]
    [InlineData(19, "Nineteen")]
    [InlineData(20, "Twenty")]
    [InlineData(45, "Forty-Five")]
    [InlineData(100, "One Hundred")]
    [InlineData(305, "Three Hundred Five")]
    [InlineData(999, "Nine Hundred Ninety-Nine")]
    public void ToWords_ValidRange_ReturnsExpectedWords(int number, string expected)
    {
        Assert.Equal(expected, number.ToWords());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void ToWords_OutOfRange_Throws(int number)
    {
        // Assert.Throws checks that calling the given code actually raises
        // the expected exception type - if it doesn't throw (or throws a
        // different type), the test fails.
        Assert.Throws<ArgumentOutOfRangeException>(() => number.ToWords());
    }
}
