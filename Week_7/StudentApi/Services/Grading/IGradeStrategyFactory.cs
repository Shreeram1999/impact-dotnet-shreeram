namespace StudentApi.Services.Grading;

// Task 5.8 - selects an IGradeStrategy per request (the ?scale= query
// parameter on GET api/students/{id}/grade). Registered behind this
// interface so a new scale can be added as a new factory implementation -
// or the switch inside GradeStrategyFactory can grow a new case - without
// StudentsController changing at all.
public interface IGradeStrategyFactory
{
    IGradeStrategy Create(string? scale);
}
