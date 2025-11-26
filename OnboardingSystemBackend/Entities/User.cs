using System;
using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string? ExternalId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int DepartmentId { get; set; }

    public int? MentorId { get; set; }

    public DateOnly HireDate { get; set; }

    public string OnboardingStatus { get; set; } = null!;

    public string? JobTitle { get; set; }

    public DateTime? RimsLastSyncDate { get; set; }

    public virtual ICollection<ActionLog> ActionLogs { get; set; } = new List<ActionLog>();

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<User> InverseMentor { get; set; } = new List<User>();

    public virtual User? Mentor { get; set; }

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();

    public virtual ICollection<UserModuleProgress> UserModuleProgresses { get; set; } = new List<UserModuleProgress>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
