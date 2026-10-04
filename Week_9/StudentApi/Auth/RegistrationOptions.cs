namespace StudentApi.Auth;

// Week 8 - self-registration (Task 8.5). Anyone may register as a Student.
// Registering as a TEACHER is off by default: if anyone could sign
// themselves up as a Teacher, the server-side role checks from Task 6.15
// would protect nothing. appsettings.Development.json turns it on so the
// role-based UI can be demoed locally with fresh accounts. A real portal
// would provision or approve teacher accounts instead.
public class RegistrationOptions
{
    public const string SectionName = "Registration";

    public bool AllowTeacherSelfRegistration { get; set; }
}
