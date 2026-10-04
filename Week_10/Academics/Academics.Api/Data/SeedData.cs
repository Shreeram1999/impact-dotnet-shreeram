using Academics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Academics.Api.Data;

// Task 10.3 - seed data via HasData (constants only, so migrations stay stable).
public static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Teacher>().HasData(
            new Teacher { Id = 1, Name = "Meera Iyer", Email = "meera.iyer@school.example", Designation = "Mathematics" },
            new Teacher { Id = 2, Name = "Arjun Rao", Email = "arjun.rao@school.example", Designation = "Physics" });

        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, Name = "Asha Kumar", Age = 20, RollNumber = "R001", Email = "asha.kumar@school.example", Score = 82, EnrolledOn = new DateOnly(2026, 6, 1) },
            new Student { Id = 2, Name = "Rohit Menon", Age = 21, RollNumber = "R002", Email = "rohit.menon@school.example", Score = 64, EnrolledOn = new DateOnly(2026, 6, 1) },
            new Student { Id = 3, Name = "Divya Nair", Age = 19, RollNumber = "R003", Email = "divya.nair@school.example", Score = 91, InternalNotes = "Scholarship review pending", EnrolledOn = new DateOnly(2026, 6, 15) });

        modelBuilder.Entity<Course>().HasData(
            new Course { Id = 1, Code = "MATH101", Title = "Calculus I", Credits = 4, TeacherId = 1 },
            new Course { Id = 2, Code = "PHYS101", Title = "Mechanics", Credits = 4, TeacherId = 2 },
            new Course { Id = 3, Code = "CS101", Title = "Programming in C#", Credits = 3 });

        modelBuilder.Entity<Enrollment>().HasData(
            new Enrollment { StudentId = 1, CourseId = 1, EnrolledOn = new DateOnly(2026, 6, 2) },
            new Enrollment { StudentId = 1, CourseId = 3, EnrolledOn = new DateOnly(2026, 6, 2) },
            new Enrollment { StudentId = 2, CourseId = 2, EnrolledOn = new DateOnly(2026, 6, 3) },
            new Enrollment { StudentId = 3, CourseId = 1, EnrolledOn = new DateOnly(2026, 6, 16) });
    }
}
