namespace OnboardingSystem.Services.Gamification;

public enum EngagementCluster
{
    Steady,
    Struggler,
    Finisher,
    Coaster
}

public class EngagementState
{
    public double ProgressRatio { get; set; }
    public double FrustrationScore { get; set; }
    public int DaysInactive { get; set; }
    public int ModulesCompletedLast7Days { get; set; }
    public int ActivityStreakDays { get; set; }
    public EngagementCluster Cluster { get; set; }
}
