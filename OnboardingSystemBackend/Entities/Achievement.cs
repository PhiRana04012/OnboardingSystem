using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public class Achievement
{
    public int AchievementId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string IconName { get; set; } = null!; 
    public string ConditionKey { get; set; } = null!;

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
