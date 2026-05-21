using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;

namespace OnboardingSystem.Services.Gamification;

public class AchievementEvaluator
{
    private readonly AppDbContext _context;

    public AchievementEvaluator(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetAchievementsToGrantAsync(
        int userId,
        GamificationXpRequest request,
        EngagementState state)
    {
        var keys = new List<string>();

        if (request.ActionType == GamificationActionTypes.ModuleRead)
        {
            var completedCount = await _context.UserModuleProgresses
                .CountAsync(p => p.UserId == userId && p.Status == "Завершён");
            if (completedCount == 1)
                keys.Add("FIRST_MODULE");
        }

        if (request.ActionType == GamificationActionTypes.TestPassed)
        {
            if (request.TestScore >= 100)
                keys.Add("TEST_100");

            var title = request.ModuleTitle ?? "";
            if (title.Contains("езопасн", StringComparison.OrdinalIgnoreCase) ||
                title.Contains("узост", StringComparison.OrdinalIgnoreCase))
            {
                keys.Add("MODULE_SAFETY");
            }

            if (request.AttemptNumber > 2 && request.PreviousBestScore is decimal prev &&
                request.TestScore > prev)
            {
                keys.Add("RECOVERY");
            }
        }

        if (state.ActivityStreakDays >= 3)
            keys.Add("STREAK_3");

        if (state.Cluster == EngagementCluster.Coaster && state.DaysInactive >= 5)
        {
            var hadRecentReturn = await _context.ActionLogs.AnyAsync(a =>
                a.UserId == userId &&
                a.Timestamp >= DateTime.UtcNow.AddHours(-2));
            if (hadRecentReturn)
                keys.Add("COMEBACK");
        }

        return keys;
    }
}
