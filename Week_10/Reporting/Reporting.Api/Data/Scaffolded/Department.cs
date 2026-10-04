using System;
using System.Collections.Generic;

namespace Reporting.Api.Data.Scaffolded;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CourseCatalog> CourseCatalogs { get; set; } = new List<CourseCatalog>();
}
