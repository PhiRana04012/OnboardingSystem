using System;
using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public partial class ActionLog
{
    public long LogId { get; set; }

    public int UserId { get; set; }

    public string ActionType { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public string? Details { get; set; }

    public virtual User User { get; set; } = null!;
}
