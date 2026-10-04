using Microsoft.EntityFrameworkCore;
using StudentApi.Models;

namespace StudentApi.Data.EfCodeFirst;

// Task 7.6 - Code First: the C# model IS the schema definition. Migrations
// (Data/EfCodeFirst/Migrations) are generated from this class and turned
// into tables by `dotnet ef database update`.
//
// Everything is configured with the Fluent API rather than data annotations
// on the entities, so Models/ stays free of persistence concerns (the same
// Student class is also used by the ADO.NET and in-memory layers, which
// know nothing about EF). Nothing here is SQL Server-specific, which is
// what lets the integration tests run the same model on SQLite in-memory.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.Property(u => u.Username).HasMaxLength(100).IsRequired();
            user.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
            user.Property(u => u.Role).HasMaxLength(20).IsRequired();
            user.Property(u => u.DisplayName).HasMaxLength(100);
            user.HasIndex(u => u.Username).IsUnique();
            user.ToTable(t => t.HasCheckConstraint("CK_Users_Role", "Role IN ('Teacher', 'Student')"));
        });

        modelBuilder.Entity<Student>(student =>
        {
            student.Property(s => s.Name).HasMaxLength(100).IsRequired();
            student.Property(s => s.RollNumber).HasMaxLength(20).IsRequired();
            student.Property(s => s.Email).HasMaxLength(256).IsRequired();
            student.Property(s => s.InternalNotes).HasMaxLength(500).IsRequired();

            // Task 7.6 - the unique Email index, plus the roll number the
            // Service already treats as unique (409 Conflict).
            student.HasIndex(s => s.Email).IsUnique();
            student.HasIndex(s => s.RollNumber).IsUnique();

            student.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Students_Age", "Age BETWEEN 5 AND 100");
                t.HasCheckConstraint("CK_Students_Score", "Score BETWEEN 0 AND 100");
            });

            // A profile MAY belong to a login; deleting the login keeps the
            // profile and just clears the link.
            student.HasOne<User>().WithOne().HasForeignKey<Student>(s => s.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Teacher>(teacher =>
        {
            teacher.Property(t => t.Name).HasMaxLength(100).IsRequired();
            teacher.Property(t => t.Email).HasMaxLength(256).IsRequired();
            teacher.Property(t => t.Designation).HasMaxLength(100).IsRequired();
            teacher.HasIndex(t => t.Email).IsUnique();
            teacher.HasOne<User>().WithOne().HasForeignKey<Teacher>(t => t.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        SeedData.Apply(modelBuilder);
    }
}
