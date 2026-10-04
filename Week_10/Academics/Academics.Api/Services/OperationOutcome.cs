namespace Academics.Api.Services;

// Task 5.5 - the three outcomes a mutating Service call can have, each
// mapping to exactly one HTTP status family in the controller:
// Success -> 200/201/204, NotFound -> 404, Conflict -> 409 (a duplicate
// roll number - a business rule DataAnnotations can't express, since it
// depends on comparing against every OTHER record, not just the one being
// validated).
public enum OperationOutcome
{
    Success,
    NotFound,
    Conflict,

    // Week 10 - a value in the request BODY references something that
    // doesn't exist (e.g. a teacherId), as opposed to the resource in the
    // URL (NotFound). Maps to 400.
    Invalid
}
