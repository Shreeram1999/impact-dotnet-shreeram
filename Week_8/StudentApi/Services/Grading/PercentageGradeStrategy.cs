namespace StudentApi.Services.Grading;

// Task 5.8 - the default strategy: the raw 0-100 score, shown as-is.
public class PercentageGradeStrategy : IGradeStrategy
{
    public string Describe(int score) => $"{score}%";
}
