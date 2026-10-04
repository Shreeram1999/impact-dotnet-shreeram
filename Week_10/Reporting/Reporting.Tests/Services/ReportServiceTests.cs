using Reporting.Api.Data.Scaffolded;
using Reporting.Api.Services;
using Reporting.Tests.TestSupport;

namespace Reporting.Tests.Services;

// The report maths, over the scaffolded context on SQLite. Every expected
// value below can be checked by hand against 02_ReportingSeedData.sql.
public sealed class ReportServiceTests : IDisposable
{
    private readonly ReportingTestDatabase database = new();

    public void Dispose() => database.Dispose();

    private ReportService Service() => new(database.CreateContext());

    [Fact]
    public void Terms_AreListedOldestFirst()
    {
        Assert.Equal(["Spring 2026", "Monsoon 2026"], Service().Terms().Select(t => t.Name));
    }

    [Fact]
    public void Summary_WithoutATerm_IsTheMostRecentTerm()
    {
        var summary = Service().EnrollmentSummary(null)!;

        Assert.Equal("Monsoon 2026", summary.Term.Name);
        Assert.Equal(9, summary.TotalEnrolled);
        Assert.Equal(5, summary.TotalCompleted);
        Assert.Equal(["CS201", "MATH201", "PHYS101"], summary.Courses.Select(c => c.CourseCode));
    }

    // CS201, Monsoon: scores 82, 90, 39 and one not in yet.
    [Fact]
    public void CourseRow_AveragesOnlyScoredRows_AndPassRateUsesTheScoredCount()
    {
        var cs201 = Service().EnrollmentSummary(2)!.Courses.Single(c => c.CourseCode == "CS201");

        Assert.Equal(new CourseSummaryDto("CS201", "Web APIs with ASP.NET Core", "Computer Science", 3, 4, 3, 70.3, 66.7), cs201);
    }

    [Fact]
    public void ACourseWithNoScoresYet_HasNullAverageAndPassRate()
    {
        using var context = database.CreateContext();
        context.EnrollmentFacts.Add(new EnrollmentFact { TermId = 2, CourseCode = "CS101", StudentRollNumber = "R009", Completed = false });
        context.SaveChanges();

        var cs101 = Service().EnrollmentSummary(2)!.Courses.Single(c => c.CourseCode == "CS101");

        Assert.Equal(1, cs101.Enrolled);
        Assert.Null(cs101.AverageScore);
        Assert.Null(cs101.PassRate);
    }

    // MATH101, Spring: 78, 35, 92, 61 -> average 66.5, 3 of 4 passed (>= 40).
    [Fact]
    public void FinishedTerm_Math101()
    {
        var math = Service().EnrollmentSummary(1)!.Courses.Single(c => c.CourseCode == "MATH101");

        Assert.Equal(4, math.Enrolled);
        Assert.Equal(66.5, math.AverageScore);
        Assert.Equal(75.0, math.PassRate);
    }

    [Fact]
    public void UnknownTerm_IsNull()
    {
        Assert.Null(Service().EnrollmentSummary(99));
        Assert.Null(Service().DepartmentSummary(99));
    }

    [Fact]
    public void DepartmentSummary_GroupsCoursesByDepartment()
    {
        var departments = Service().DepartmentSummary(1)!;

        Assert.Equal(
            [
                new DepartmentSummaryDto("Computer Science", 1, 4, 79.3),
                new DepartmentSummaryDto("Mathematics", 1, 4, 66.5),
                new DepartmentSummaryDto("Physics", 1, 3, 54.3)
            ],
            departments);
    }

    [Fact]
    public void EmptyWarehouse_HasNoTermsAndNoSummary()
    {
        using var empty = new ReportingTestDatabase(seed: false);
        var service = new ReportService(empty.CreateContext());

        Assert.Empty(service.Terms());
        Assert.Null(service.EnrollmentSummary(null));
    }
}
