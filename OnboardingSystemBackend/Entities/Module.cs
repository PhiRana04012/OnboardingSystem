using System;
using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public partial class Module
{
    public int ModuleId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Content { get; set; }

    public bool IsMandatory { get; set; }

    public int? DepartmentId { get; set; }

    public int PassingScore { get; set; }

    public int MaxAttempts { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();

    public virtual ICollection<UserModuleProgress> UserModuleProgresses { get; set; } = new List<UserModuleProgress>();
}
