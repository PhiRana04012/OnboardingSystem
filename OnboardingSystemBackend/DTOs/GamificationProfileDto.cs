using System;
using System.Collections.Generic;

namespace OnboardingSystem.DTOs;

public class GamificationProfileDto 
{
    public int UserId { get; set; }
    public int TotalXP { get; set; }
    public int Level { get; set; }
    public int NextLevelXP { get; set; }
    public List<AchievementDto> EarnedAchievements { get; set; } = new();
    public List<AchievementDto> AllAchievements { get; set; } = new();
}

public class AchievementDto
{
    public int AchievementId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string IconName { get; set; } = null!;
    public bool IsEarned { get; set; }
    public DateTime? AwardedAt { get; set; }
}
