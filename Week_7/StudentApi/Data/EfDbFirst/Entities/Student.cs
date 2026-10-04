using System;
using System.Collections.Generic;

namespace StudentApi.Data.EfDbFirst.Entities;

public partial class Student
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Age { get; set; }

    public string RollNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int Score { get; set; }

    public string InternalNotes { get; set; } = null!;

    public DateOnly? EnrolledOn { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
