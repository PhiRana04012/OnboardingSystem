using System;

namespace OnboardingSystem.Entities;

public class UserAchievement
{
    public int UserAchievementId { get; set; }
    public int UserId { get; set; }
    public int AchievementId { get; set; }
    public DateTime AwardedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Achievement Achievement { get; set; } = null!;
}
