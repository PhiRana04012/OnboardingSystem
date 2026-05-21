namespace OnboardingSystem.Entities;

public class XpGrantLog
{
    public long XpGrantId { get; set; }
    public int UserId { get; set; }
    public string ActionType { get; set; } = null!;
    public int? ModuleId { get; set; }
    public int BaseXp { get; set; }
    public decimal Multiplier { get; set; } = 1m;
    public int FinalXp { get; set; }
    public string? ReasonSummary { get; set; }
    public DateTime GrantedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
