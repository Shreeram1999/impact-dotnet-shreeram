namespace StudentApi.Services.Grading;

// Task 5.8 - the alternate strategy: the same 0-100 score, rescaled onto a
// 4.0 GPA scale instead. StudentsController.GetGrade doesn't know this math
// exists - it only calls IGradeStrategy.Describe(score), whichever
// implementation the factory handed it.
public class GpaGradeStrategy : IGradeStrategy
{
    public string Describe(int score)
    {
        var gpa = score / 100.0 * 4.0;
        return $"{gpa:0.00} GPA";
    }
}
