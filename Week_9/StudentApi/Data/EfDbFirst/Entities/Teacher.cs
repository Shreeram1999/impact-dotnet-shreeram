using System;
using System.Collections.Generic;

namespace StudentApi.Data.EfDbFirst.Entities;

public partial class Teacher
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
