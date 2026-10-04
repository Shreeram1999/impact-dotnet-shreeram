using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Reporting.Api.Data.Scaffolded;

namespace Reporting.Api.Data;

public partial class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CourseCatalog> CourseCatalogs { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<EnrollmentFact> EnrollmentFacts { get; set; }

    public virtual DbSet<Term> Terms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseCatalog>(entity =>
        {
            entity.HasKey(e => e.CourseCode);

            entity.ToTable("CourseCatalog");

            entity.Property(e => e.CourseCode).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Department).WithMany(p => p.CourseCatalogs)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseCatalog_Departments");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Departments_Name").IsUnique();

            entity.Property(e => e.DepartmentId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<EnrollmentFact>(entity =>
        {
            entity.HasIndex(e => new { e.TermId, e.CourseCode, e.StudentRollNumber }, "UQ_EnrollmentFacts_Term_Course_Student").IsUnique();

            entity.Property(e => e.CourseCode).HasMaxLength(20);
            entity.Property(e => e.StudentRollNumber).HasMaxLength(20);

            entity.HasOne(d => d.CourseCodeNavigation).WithMany(p => p.EnrollmentFacts)
                .HasForeignKey(d => d.CourseCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EnrollmentFacts_CourseCatalog");

            entity.HasOne(d => d.Term).WithMany(p => p.EnrollmentFacts)
                .HasForeignKey(d => d.TermId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EnrollmentFacts_Terms");
        });

        modelBuilder.Entity<Term>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Terms_Name").IsUnique();

            entity.Property(e => e.TermId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
