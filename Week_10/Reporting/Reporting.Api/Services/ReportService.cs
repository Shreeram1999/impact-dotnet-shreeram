using Microsoft.EntityFrameworkCore;
using Reporting.Api.Data;

namespace Reporting.Api.Services;

public record TermDto(int TermId, string Name, DateOnly StartsOn, DateOnly EndsOn);

public record CourseSummaryDto(
    string CourseCode, string Title, string Department, int Credits,
    int Enrolled, int Completed, double? AverageScore, double? PassRate);

public record DepartmentSummaryDto(string Department, int Courses, int Enrolled, double? AverageScore);

public record EnrollmentSummaryDto(TermDto Term, int TotalEnrolled, int TotalCompleted, IReadOnlyList<CourseSummaryDto> Courses);

public interface IReportService
{
    IReadOnlyList<TermDto> Terms();
    EnrollmentSummaryDto? EnrollmentSummary(int? termId);
    IReadOnlyList<DepartmentSummaryDto>? DepartmentSummary(int? termId);
}

// Task 10.4 - read-mostly reporting over the scaffolded context. The
// aggregations are written as LINQ GroupBy projections, so EF translates them
// into one SQL GROUP BY per report instead of pulling fact rows into memory.
// Only aggregates leave this service: no per-student rows, so any signed-in
// user may see them.
public class ReportService : IReportService
{
    public const int PassMark = 40;

    private readonly ReportingDbContext context;

    public ReportService(ReportingDbContext context)
    {
        this.context = context;
    }

    public IReadOnlyList<TermDto> Terms() =>
        context.Terms.OrderBy(t => t.StartsOn)
            .Select(t => new TermDto(t.TermId, t.Name, t.StartsOn, t.EndsOn))
            .ToList();

    // termId omitted -> the most recent term. Unknown termId -> null (404).
    public EnrollmentSummaryDto? EnrollmentSummary(int? termId)
    {
        var term = FindTerm(termId);
        if (term is null)
            return null;

        var courses = context.EnrollmentFacts
            .Where(f => f.TermId == term.TermId)
            .GroupBy(f => new
            {
                f.CourseCode,
                f.CourseCodeNavigation.Title,
                Department = f.CourseCodeNavigation.Department.Name,
                f.CourseCodeNavigation.Credits
            })
            .Select(g => new
            {
                g.Key,
                Enrolled = g.Count(),
                Completed = g.Count(f => f.Completed),
                Average = g.Average(f => (double?)f.FinalScore),
                Scored = g.Count(f => f.FinalScore != null),
                Passed = g.Count(f => f.FinalScore >= PassMark)
            })
            .OrderBy(x => x.Key.CourseCode)
            .ToList()
            .Select(x => new CourseSummaryDto(
                x.Key.CourseCode, x.Key.Title, x.Key.Department, x.Key.Credits,
                x.Enrolled, x.Completed,
                x.Average is { } avg ? Round(avg) : null,
                x.Scored == 0 ? null : Round(100.0 * x.Passed / x.Scored)))
            .ToList();

        return new EnrollmentSummaryDto(term, courses.Sum(c => c.Enrolled), courses.Sum(c => c.Completed), courses);
    }

    public IReadOnlyList<DepartmentSummaryDto>? DepartmentSummary(int? termId)
    {
        var term = FindTerm(termId);
        if (term is null)
            return null;

        return context.EnrollmentFacts
            .Where(f => f.TermId == term.TermId)
            .GroupBy(f => f.CourseCodeNavigation.Department.Name)
            .Select(g => new
            {
                Department = g.Key,
                Courses = g.Select(f => f.CourseCode).Distinct().Count(),
                Enrolled = g.Count(),
                Average = g.Average(f => (double?)f.FinalScore)
            })
            .OrderBy(x => x.Department)
            .ToList()
            .Select(x => new DepartmentSummaryDto(x.Department, x.Courses, x.Enrolled, x.Average is { } avg ? Round(avg) : null))
            .ToList();
    }

    // Reports round half AWAY from zero (79.25 -> 79.3), as people and SQL
    // Server's ROUND expect - not .NET's default banker's rounding (79.2).
    private static double Round(double value) => Math.Round(value, 1, MidpointRounding.AwayFromZero);

    private TermDto? FindTerm(int? termId)
    {
        var terms = context.Terms.AsQueryable();
        var term = termId is { } id
            ? terms.FirstOrDefault(t => t.TermId == id)
            : terms.OrderByDescending(t => t.StartsOn).FirstOrDefault();
        return term is null ? null : new TermDto(term.TermId, term.Name, term.StartsOn, term.EndsOn);
    }
}
