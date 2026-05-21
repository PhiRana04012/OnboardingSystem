namespace OnboardingSystem.DTOs;

public class AnalyticsDashboardDto
{
    public AIAnalyticsDto Analytics { get; set; } = new();
    public LearningPathDto LearningPath { get; set; } = new();
    public PlannedModuleDto? NextModule { get; set; }
}
