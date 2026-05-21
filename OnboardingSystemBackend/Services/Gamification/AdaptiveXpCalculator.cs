namespace OnboardingSystem.Services.Gamification;

public class AdaptiveXpCalculator
{
    public (int baseXp, decimal multiplier, string reason) Calculate(
        EngagementState state,
        GamificationXpRequest request,
        int sameActionTodayCount,
        bool firstCompletionForModule)
    {
        return request.ActionType switch
        {
            GamificationActionTypes.ModuleRead => CalculateModuleRead(state, request, sameActionTodayCount, firstCompletionForModule),
            GamificationActionTypes.TestPassed => CalculateTestPassed(state, request, sameActionTodayCount),
            GamificationActionTypes.TestFailed => CalculateTestFailed(state, request),
            _ => (0, 1m, "")
        };
    }

    private static (int, decimal, string) CalculateModuleRead(
        EngagementState state,
        GamificationXpRequest request,
        int sameActionTodayCount,
        bool firstCompletion)
    {
        var baseXp = request.IsMandatoryModule ? 15 : 10;
        var multiplier = 1m;
        var reasons = new List<string>();

        if (!firstCompletion)
        {
            multiplier *= 0.35m;
            reasons.Add("повторное изучение");
        }

        if (sameActionTodayCount >= 2)
        {
            multiplier *= 0.2m;
            reasons.Add("лимит активности за день");
        }

        switch (state.Cluster)
        {
            case EngagementCluster.Struggler:
                multiplier *= 1.25m;
                reasons.Add("поддержка прогресса");
                break;
            case EngagementCluster.Finisher:
                multiplier *= 0.85m;
                reasons.Add("фокус на сложных задачах");
                break;
            case EngagementCluster.Coaster when state.DaysInactive >= 3:
                multiplier *= 1.35m;
                reasons.Add("возвращение в обучение");
                break;
        }

        multiplier = Math.Clamp(multiplier, 0.1m, 1.5m);
        var reason = reasons.Count > 0 ? string.Join(", ", reasons) : "изучение материала";
        return (baseXp, multiplier, reason);
    }

    private static (int, decimal, string) CalculateTestPassed(
        EngagementState state,
        GamificationXpRequest request,
        int sameActionTodayCount)
    {
        var score = request.TestScore ?? 0;
        var baseXp = 45;
        if (request.AttemptNumber == 1) baseXp += 15;
        if (score >= 100) baseXp += 25;
        else if (score >= 90) baseXp += 10;

        var multiplier = 1m;
        var reasons = new List<string> { "успешный тест" };

        if (request.PreviousBestScore is decimal prev && score > prev + 5)
        {
            multiplier *= 1.2m;
            reasons.Add("улучшение результата");
        }

        if (state.Cluster == EngagementCluster.Struggler && request.AttemptNumber > 1)
        {
            multiplier *= 1.3m;
            reasons.Add("преодоление трудностей");
        }

        if (state.Cluster == EngagementCluster.Finisher && score >= 95)
        {
            multiplier *= 0.9m;
        }

        if (sameActionTodayCount >= 3)
        {
            multiplier *= 0.5m;
            reasons.Add("лимит попыток за день");
        }

        multiplier = Math.Clamp(multiplier, 0.2m, 1.6m);
        return (baseXp, multiplier, string.Join(", ", reasons));
    }

    private static (int, decimal, string) CalculateTestFailed(
        EngagementState state,
        GamificationXpRequest request)
    {
        var baseXp = 4;
        var multiplier = 1m;
        var reasons = new List<string> { "участие в тесте" };

        if (request.PreviousBestScore is decimal prev &&
            request.TestScore is decimal score &&
            score > prev)
        {
            baseXp = 8;
            multiplier = 1.25m;
            reasons.Add("прогресс к зачёту");
        }

        if (state.Cluster == EngagementCluster.Struggler)
        {
            multiplier *= 1.15m;
            reasons.Add("мотивация продолжать");
        }

        return (baseXp, Math.Clamp(multiplier, 0.5m, 1.5m), string.Join(", ", reasons));
    }
}
