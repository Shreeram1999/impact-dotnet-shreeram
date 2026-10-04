namespace StudentApi.Services;

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
    Conflict
}
