using Microsoft.EntityFrameworkCore;
using StudentApi.Auth;
using StudentApi.Models;

namespace StudentApi.Data.EfCodeFirst;

// Task 7.6 - seed data via HasData. These are the same rows as
// Database/04_SeedData.sql, so every data layer starts identical. HasData
// values must be constants (a random value would make every new migration
// "change" the seed), which is why the PBKDF2 hashes are precomputed
// strings rather than hasher.Hash(...) calls.
public static class SeedData
{
    public const string TeacherPasswordHash = "PBKDF2-SHA256$100000$J0Zb75kU8S4WKblBDAMrYQ==$7o3+1kfT10l09ygTNfciYKy0t9ZQzlTN49koV4hImbQ=";
    public const string StudentPasswordHash = "PBKDF2-SHA256$100000$IvdcCNnm9IZEJ6iNMbW1fQ==$A8dqeb89Q4z+7vZRGjpEH3Iy+X46px+29wuxFOx3SZI=";

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "teacher1", PasswordHash = TeacherPasswordHash, Role = Roles.Teacher },
            new User { Id = 2, Username = "student1", PasswordHash = StudentPasswordHash, Role = Roles.Student });

        modelBuilder.Entity<Teacher>().HasData(
            new Teacher { Id = 1, Name = "Meera Iyer", Email = "meera.iyer@school.example", Designation = "Mathematics", UserId = 1 },
            new Teacher { Id = 2, Name = "Arjun Rao", Email = "arjun.rao@school.example", Designation = "Physics" });

        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, Name = "Asha Kumar", Age = 20, RollNumber = "R001", Email = "asha.kumar@school.example", Score = 82, UserId = 2, EnrolledOn = new DateOnly(2026, 6, 1) },
            new Student { Id = 2, Name = "Rohit Menon", Age = 21, RollNumber = "R002", Email = "rohit.menon@school.example", Score = 64, EnrolledOn = new DateOnly(2026, 6, 1) },
            new Student { Id = 3, Name = "Divya Nair", Age = 19, RollNumber = "R003", Email = "divya.nair@school.example", Score = 91, InternalNotes = "Scholarship review pending", EnrolledOn = new DateOnly(2026, 6, 15) });
    }
}
