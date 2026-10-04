namespace StudentApi.Services.Grading;

// Task 5.8 - "percentage" (also the default, for an omitted or unrecognized
// scale) and "gpa", matched case-insensitively so ?scale=GPA and
// ?scale=gpa behave the same. Falling back to Percentage instead of
// throwing on an unknown value keeps the endpoint a plain 200 for any input
// - there's no invalid "scale" from the caller's point of view, just a
// choice of default.
public class GradeStrategyFactory : IGradeStrategyFactory
{
    public IGradeStrategy Create(string? scale) =>
        scale?.Trim().ToLowerInvariant() switch
        {
            "gpa" => new GpaGradeStrategy(),
            _ => new PercentageGradeStrategy()
        };
}
