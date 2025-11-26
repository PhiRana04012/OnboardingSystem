using System;
using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public partial class TestAttempt
{
    public long AttemptId { get; set; }

    public int UserId { get; set; }

    public int ModuleId { get; set; }

    public DateTime AttemptDate { get; set; }

    public int AttemptNumber { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }

    public virtual Module Module { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
