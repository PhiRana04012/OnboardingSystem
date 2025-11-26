using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActionLog> ActionLogs { get; set; }

    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TestAttempt> TestAttempts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserModuleProgress> UserModuleProgresses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is configured in Program.cs via DI
        // This method is only called if options were not provided via constructor
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=onboarding;Integrated Security=true;TrustServerCertificate=true;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__ActionLo__5E5499A8B7BEDF7C");

            entity.ToTable("ActionLog");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.ActionType).HasMaxLength(100);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.ActionLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActionLog_Users");
        });

        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__AnswerOp__D482502408F6A591");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.AnswerText).HasMaxLength(500);
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_AnswerOptions_Questions");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BCD767D3C89");

            entity.HasIndex(e => e.Name, "UQ__Departme__737584F630BC325D").IsUnique();

            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.ExternalId)
                .HasMaxLength(100)
                .HasColumnName("ExternalID");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.ModuleId).HasName("PK__Modules__2B747787B4D47241");

            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.IsMandatory).HasDefaultValue(true);
            entity.Property(e => e.MaxAttempts).HasDefaultValue(3);
            entity.Property(e => e.PassingScore).HasDefaultValue(80);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Department).WithMany(p => p.Modules)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_Modules_Departments");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8C277F87A3");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
            entity.Property(e => e.QuestionText).HasMaxLength(1000);

            entity.HasOne(d => d.Module).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK_Questions_Modules");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A04F18E8C");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616047850BFA").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<TestAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("PK__TestAtte__891A68868F130401");

            entity.Property(e => e.AttemptId).HasColumnName("AttemptID");
            entity.Property(e => e.AttemptDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Module).WithMany(p => p.TestAttempts)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK_TestAttempts_Modules");

            entity.HasOne(d => d.User).WithMany(p => p.TestAttempts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_TestAttempts_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC41930BCF");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534CAA8D9F8").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.ExternalId)
                .HasMaxLength(100)
                .HasColumnName("ExternalID");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.MentorId).HasColumnName("MentorID");
            entity.Property(e => e.OnboardingStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Не начат");
            entity.Property(e => e.JobTitle).HasMaxLength(255);
            entity.Property(e => e.RimsLastSyncDate).HasColumnName("RimsLastSyncDate");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Departments");

            entity.HasOne(d => d.Mentor).WithMany(p => p.InverseMentor)
                .HasForeignKey(d => d.MentorId)
                .HasConstraintName("FK_Users_Mentor");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_UserRoles_Roles"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_UserRoles_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF27604F7D17785F");
                        j.ToTable("UserRoles");
                        j.IndexerProperty<int>("UserId").HasColumnName("UserID");
                        j.IndexerProperty<int>("RoleId").HasColumnName("RoleID");
                    });
        });

        modelBuilder.Entity<UserModuleProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("PK__UserModu__BAE29C85994BA69E");

            entity.ToTable("UserModuleProgress");

            entity.HasIndex(e => new { e.UserId, e.ModuleId }, "UQ__UserModu__753F8BD5DAF2E324").IsUnique();

            entity.Property(e => e.ProgressId).HasColumnName("ProgressID");
            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Не начат");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Module).WithMany(p => p.UserModuleProgresses)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK_UserModuleProgress_Modules");

            entity.HasOne(d => d.User).WithMany(p => p.UserModuleProgresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserModuleProgress_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
