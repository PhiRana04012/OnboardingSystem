namespace OnboardingSystem.Services.Gamification;

public class GamificationXpRequest
{
    public string ActionType { get; set; } = null!;
    public int? ModuleId { get; set; }
    public bool IsMandatoryModule { get; set; }
    public decimal? TestScore { get; set; }
    public int? AttemptNumber { get; set; }
    public bool TestPassed { get; set; }
    public decimal? PreviousBestScore { get; set; }
    public string? ModuleTitle { get; set; }
}

public class XpGrantResult
{
    public int FinalXp { get; set; }
    public int BaseXp { get; set; }
    public decimal Multiplier { get; set; }
    public string ReasonSummary { get; set; } = "";
    public int TotalXpAfter { get; set; }
    public int LevelAfter { get; set; }
    public bool LevelUp { get; set; }
}
