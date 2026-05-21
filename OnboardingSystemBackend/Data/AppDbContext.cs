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

    public virtual DbSet<JobTitle> JobTitles { get; set; }

    public virtual DbSet<Module> Modules { get; set; }
    
    public virtual DbSet<ModuleDepartment> ModuleDepartments { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TestAttempt> TestAttempts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserModuleProgress> UserModuleProgresses { get; set; }
    
    public virtual DbSet<Achievement> Achievements { get; set; }
    
    public virtual DbSet<UserAchievement> UserAchievements { get; set; }
    
    public virtual DbSet<ChecklistItem> ChecklistItems { get; set; }
    
    public virtual DbSet<UserChecklistItem> UserChecklistItems { get; set; }
    
    public virtual DbSet<FaqEntry> FaqEntries { get; set; }
    
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<XpGrantLog> XpGrantLogs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

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
            entity.Property(e => e.HeadUserId).HasColumnName("HeadUserID");

            entity.HasOne(d => d.Head)
                .WithMany()
                .HasForeignKey(d => d.HeadUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Departments_Users_Head");
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

        modelBuilder.Entity<ModuleDepartment>(entity =>
        {
            entity.ToTable("ModuleDepartments");
            entity.HasKey(e => new { e.ModuleId, e.DepartmentId });
            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

            entity.HasOne(d => d.Module)
                .WithMany(p => p.ModuleDepartments)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK_ModuleDepartments_Modules");

            entity.HasOne(d => d.Department)
                .WithMany(p => p.ModuleDepartments)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_ModuleDepartments_Departments");
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

        modelBuilder.Entity<JobTitle>(entity =>
        {
            entity.HasKey(e => e.JobTitleId).HasName("PK__JobTitle__C64C6E0DB3EE15E9");

            entity.HasIndex(e => e.Title, "UQ__JobTitle__A1D5E8A64E51E3C0").IsUnique();

            entity.Property(e => e.JobTitleId).HasColumnName("JobTitleID");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
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
            entity.Property(e => e.JobTitleId).HasColumnName("JobTitleID");
            entity.Property(e => e.RimsLastSyncDate).HasColumnName("RimsLastSyncDate");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Departments");

            entity.HasOne(d => d.JobTitle).WithMany(p => p.Users)
                .HasForeignKey(d => d.JobTitleId)
                .HasConstraintName("FK_Users_JobTitles");

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

        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(e => e.AchievementId);
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.ConditionKey).HasMaxLength(100);
            entity.HasIndex(e => e.ConditionKey).IsUnique();
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasKey(e => e.UserAchievementId);
            entity.Property(e => e.AwardedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Achievement)
                .WithMany(p => p.UserAchievements)
                .HasForeignKey(d => d.AchievementId)
                .HasConstraintName("FK_UserAchievements_Achievements");

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserAchievements)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserAchievements_Users");
                
            entity.HasIndex(e => new { e.UserId, e.AchievementId }).IsUnique();
        });

        modelBuilder.Entity<ChecklistItem>(entity =>
        {
            entity.HasKey(e => e.ChecklistItemId);
            entity.Property(e => e.Text).HasMaxLength(500);

            entity.HasOne(d => d.Module)
                .WithMany(p => p.ChecklistItems)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK_ChecklistItems_Modules");
        });

        modelBuilder.Entity<UserChecklistItem>(entity =>
        {
            entity.HasKey(e => e.UserChecklistItemId);
            entity.HasOne(d => d.User)
                .WithMany(p => p.UserChecklistItems)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserChecklistItems_Users");

            entity.HasOne(d => d.ChecklistItem)
                .WithMany()
                .HasForeignKey(d => d.ChecklistItemId)
                .HasConstraintName("FK_UserChecklistItems_ChecklistItems");
                
            entity.HasIndex(e => new { e.UserId, e.ChecklistItemId }).IsUnique();
        });

        modelBuilder.Entity<FaqEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Question).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Answer).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("Общее");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<XpGrantLog>(entity =>
        {
            entity.HasKey(e => e.XpGrantId);
            entity.Property(e => e.ActionType).HasMaxLength(50);
            entity.Property(e => e.ReasonSummary).HasMaxLength(500);
            entity.Property(e => e.Multiplier).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.GrantedAt).HasDefaultValueSql("(getutcdate())");
            entity.HasIndex(e => new { e.UserId, e.GrantedAt });

            entity.HasOne(d => d.User)
                .WithMany(p => p.XpGrantLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_XpGrantLogs_Users");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenId);
            entity.Property(e => e.Token).HasMaxLength(64).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            entity.HasIndex(e => e.Token).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PasswordResetTokens_Users");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.LinkUrl).HasMaxLength(500);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt });

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Notifications_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
