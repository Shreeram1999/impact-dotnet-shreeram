using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StudentApi.Data.EfDbFirst.Entities;

namespace StudentApi.Data.EfDbFirst;

public partial class StudentPortalDbFirstContext : DbContext
{
    public StudentPortalDbFirstContext(DbContextOptions<StudentPortalDbFirstContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Students_Email").IsUnique();

            entity.HasIndex(e => e.RollNumber, "UQ_Students_RollNumber").IsUnique();

            entity.HasIndex(e => e.UserId, "UX_Students_UserId")
                .IsUnique()
                .HasFilter("([UserId] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.InternalNotes)
                .HasMaxLength(500)
                .HasDefaultValue("", "DF_Students_InternalNotes");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RollNumber).HasMaxLength(20);

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Students_Users");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Teachers_Email").IsUnique();

            entity.HasIndex(e => e.UserId, "UX_Teachers_UserId")
                .IsUnique()
                .HasFilter("([UserId] IS NOT NULL)");

            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Teachers_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();

            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
