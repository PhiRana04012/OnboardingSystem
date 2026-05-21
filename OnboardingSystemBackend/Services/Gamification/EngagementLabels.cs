namespace OnboardingSystem.Services.Gamification;

public static class EngagementLabels
{
    public static string ToDisplayName(EngagementCluster cluster) => cluster switch
    {
        EngagementCluster.Struggler => "Активное обучение",
        EngagementCluster.Finisher => "Финишная прямая",
        EngagementCluster.Coaster => "Возвращение в ритм",
        _ => "Стабильный прогресс"
    };
}
