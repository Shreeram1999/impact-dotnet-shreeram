using Academics.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Academics.Api.Data;

// Task 10.3 - EF Code First for the domain this team owns and keeps
// changing (Week 7's verdict). AcademicsDb belongs to this service only.
// Fluent API only, and nothing SQL Server-specific, so the tests run the
// same model on SQLite in-memory.
public class AcademicsDbContext : DbContext
{
    public AcademicsDbContext(DbContextOptions<AcademicsDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(student =>
        {
            student.Property(s => s.Name).HasMaxLength(100).IsRequired();
            student.Property(s => s.RollNumber).HasMaxLength(20).IsRequired();
            student.Property(s => s.Email).HasMaxLength(256).IsRequired();
            student.Property(s => s.InternalNotes).HasMaxLength(500).IsRequired();
            student.HasIndex(s => s.RollNumber).IsUnique();
            student.HasIndex(s => s.Email).IsUnique();
            student.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Students_Age", "Age BETWEEN 5 AND 100");
                t.HasCheckConstraint("CK_Students_Score", "Score BETWEEN 0 AND 100");
            });
        });

        modelBuilder.Entity<Teacher>(teacher =>
        {
            teacher.Property(t => t.Name).HasMaxLength(100).IsRequired();
            teacher.Property(t => t.Email).HasMaxLength(256).IsRequired();
            teacher.Property(t => t.Designation).HasMaxLength(100).IsRequired();
            teacher.HasIndex(t => t.Email).IsUnique();
        });

        modelBuilder.Entity<Course>(course =>
        {
            course.Property(c => c.Code).HasMaxLength(20).IsRequired();
            course.Property(c => c.Title).HasMaxLength(150).IsRequired();
            course.HasIndex(c => c.Code).IsUnique();
            course.ToTable(t => t.HasCheckConstraint("CK_Courses_Credits", "Credits BETWEEN 1 AND 10"));

            // Deleting a teacher leaves their courses unassigned.
            course.HasOne<Teacher>().WithMany().HasForeignKey(c => c.TeacherId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Enrollment>(enrollment =>
        {
            enrollment.HasKey(e => new { e.StudentId, e.CourseId });

            // Deleting a student or a course removes their enrollments.
            enrollment.HasOne<Student>().WithMany(s => s.Enrollments).HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Cascade);
            enrollment.HasOne<Course>().WithMany(c => c.Enrollments).HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Cascade);
        });

        SeedData.Apply(modelBuilder);
    }
}
