namespace StudentApi.Auth;

// Task 6.15 - the two roles the portal knows about. Constants rather than
// magic strings so [Authorize(Roles = Roles.Teacher)] can't be mistyped.
public static class Roles
{
    public const string Teacher = "Teacher";
    public const string Student = "Student";
}
