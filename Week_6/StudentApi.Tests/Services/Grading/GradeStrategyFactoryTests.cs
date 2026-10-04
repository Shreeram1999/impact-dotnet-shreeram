using StudentApi.Services.Grading;

namespace StudentApi.Tests.Services.Grading;

// Task 5.8 - the SELECTION logic half of the Testing Focus: which scale
// string produces which concrete IGradeStrategy.
public class GradeStrategyFactoryTests
{
    private readonly GradeStrategyFactory factory = new();

    [Theory]
    [InlineData("gpa")]
    [InlineData("GPA")]
    [InlineData(" Gpa ")]
    public void Create_GpaVariants_ReturnsGpaStrategy(string scale)
    {
        Assert.IsType<GpaGradeStrategy>(factory.Create(scale));
    }

    [Theory]
    [InlineData("percentage")]
    [InlineData("PERCENTAGE")]
    [InlineData("unknown-scale")]
    [InlineData(null)]
    [InlineData("")]
    public void Create_PercentageOrUnrecognizedOrMissing_FallsBackToPercentageStrategy(string? scale)
    {
        Assert.IsType<PercentageGradeStrategy>(factory.Create(scale));
    }
}
