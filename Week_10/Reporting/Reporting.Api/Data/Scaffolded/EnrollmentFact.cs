using System;
using System.Collections.Generic;

namespace Reporting.Api.Data.Scaffolded;

public partial class EnrollmentFact
{
    public int EnrollmentFactId { get; set; }

    public int TermId { get; set; }

    public string CourseCode { get; set; } = null!;

    public string StudentRollNumber { get; set; } = null!;

    public int? FinalScore { get; set; }

    public bool Completed { get; set; }

    public virtual CourseCatalog CourseCodeNavigation { get; set; } = null!;

    public virtual Term Term { get; set; } = null!;
}
