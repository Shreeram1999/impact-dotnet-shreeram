using StudentApi.Services.Grading;

namespace StudentApi.Tests.Services.Grading;

// Task 5.8 - each strategy's OUTPUT, per the Testing Focus ("each
// strategy's output + selection logic").
public class GradeStrategyTests
{
    [Theory]
    [InlineData(0, "0%")]
    [InlineData(82, "82%")]
    [InlineData(100, "100%")]
    public void PercentageGradeStrategy_ReturnsScoreAsAPercentage(int score, string expected)
    {
        IGradeStrategy strategy = new PercentageGradeStrategy();

        Assert.Equal(expected, strategy.Describe(score));
    }

    [Theory]
    [InlineData(0, "0.00 GPA")]
    [InlineData(100, "4.00 GPA")]
    [InlineData(82, "3.28 GPA")]
    [InlineData(50, "2.00 GPA")]
    public void GpaGradeStrategy_RescalesScoreOnto4PointScale(int score, string expected)
    {
        IGradeStrategy strategy = new GpaGradeStrategy();

        Assert.Equal(expected, strategy.Describe(score));
    }
}
