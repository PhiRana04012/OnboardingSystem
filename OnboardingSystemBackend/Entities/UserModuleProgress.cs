using System;
using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public partial class UserModuleProgress
{
    public int ProgressId { get; set; }

    public int UserId { get; set; }

    public int ModuleId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? CompletionDate { get; set; }

    public virtual Module Module { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
