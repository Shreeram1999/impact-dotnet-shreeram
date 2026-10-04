using System;
using System.Collections.Generic;

namespace Reporting.Api.Data.Scaffolded;

public partial class Term
{
    public int TermId { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly StartsOn { get; set; }

    public DateOnly EndsOn { get; set; }

    public virtual ICollection<EnrollmentFact> EnrollmentFacts { get; set; } = new List<EnrollmentFact>();
}
