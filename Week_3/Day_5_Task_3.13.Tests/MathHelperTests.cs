// This is the file the "TESTING FOCUS" note specifically calls out edge
// cases for: 0, 1, and negative numbers, for all three MathHelper methods.
public class MathHelperTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    [InlineData(6, 720)]
    public void Factorial_ValidInputs_ReturnsExpectedResult(int input, long expected)
    {
        Assert.Equal(expected, MathHelper.Factorial(input));
    }

    [Fact]
    public void Factorial_NegativeNumber_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MathHelper.Factorial(-1));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(-5, false)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(17, true)]
    public void IsPrime_VariousInputs_ReturnsExpectedResult(int input, bool expected)
    {
        Assert.Equal(expected, MathHelper.IsPrime(input));
    }

    [Theory]
    [InlineData(48, 18, 6)]
    [InlineData(0, 5, 5)]
    [InlineData(5, 0, 5)]
    [InlineData(-12, 8, 4)]
    [InlineData(7, 13, 1)]
    public void GCD_VariousInputs_ReturnsExpectedResult(int a, int b, int expected)
    {
        Assert.Equal(expected, MathHelper.GCD(a, b));
    }

    [Fact]
    public void GCD_BothZero_Throws()
    {
        Assert.Throws<ArgumentException>(() => MathHelper.GCD(0, 0));
    }
}
