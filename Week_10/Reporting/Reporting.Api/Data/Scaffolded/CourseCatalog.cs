using System;
using System.Collections.Generic;

namespace Reporting.Api.Data.Scaffolded;

public partial class CourseCatalog
{
    public string CourseCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int DepartmentId { get; set; }

    public int Credits { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<EnrollmentFact> EnrollmentFacts { get; set; } = new List<EnrollmentFact>();
}
