using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.Entities;
using OnboardingSystem.Services.Gamification;

namespace OnboardingSystem.Services;

public interface IGamificationService
{
    Task<XpGrantResult> AwardXpAsync(int userId, GamificationXpRequest request);
    Task<XpGrantResult> ProcessEventAsync(int userId, GamificationXpRequest request);
    Task AddXpAsync(int userId, int xpAmount);
    Task GrantAchievementAsync(int userId, string conditionKey);
    Task CheckAndGrantLevelAchievementsAsync(int userId);
    Task<EngagementState> GetEngagementStateAsync(int userId);
}

public class GamificationService : IGamificationService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly EngagementProfileBuilder _engagementBuilder;
    private readonly AdaptiveXpCalculator _xpCalculator;
    private readonly AchievementEvaluator _achievementEvaluator;

    public GamificationService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
        _engagementBuilder = new EngagementProfileBuilder(context);
        _xpCalculator = new AdaptiveXpCalculator();
        _achievementEvaluator = new AchievementEvaluator(context);
    }

    public async Task<EngagementState> GetEngagementStateAsync(int userId) =>
        await _engagementBuilder.BuildAsync(userId);

    public async Task<XpGrantResult> ProcessEventAsync(int userId, GamificationXpRequest request)
    {
        var result = await AwardXpAsync(userId, request);

        var state = await _engagementBuilder.BuildAsync(userId);
        var keys = await _achievementEvaluator.GetAchievementsToGrantAsync(userId, request, state);
        foreach (var key in keys.Distinct())
            await GrantAchievementAsync(userId, key);

        return result;
    }

    public async Task<XpGrantResult> AwardXpAsync(int userId, GamificationXpRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return new XpGrantResult();

        var state = await _engagementBuilder.BuildAsync(userId);
        var today = DateTime.UtcNow.Date;

        var sameActionToday = await _context.XpGrantLogs.CountAsync(x =>
            x.UserId == userId &&
            x.ActionType == request.ActionType &&
            x.ModuleId == request.ModuleId &&
            x.GrantedAt >= today);

        var firstCompletion = request.ModuleId == null || !await _context.XpGrantLogs.AnyAsync(x =>
            x.UserId == userId &&
            x.ActionType == GamificationActionTypes.ModuleRead &&
            x.ModuleId == request.ModuleId);

        var (baseXp, multiplier, reason) = _xpCalculator.Calculate(
            state, request, sameActionToday, firstCompletion);

        var finalXp = (int)Math.Round(baseXp * multiplier);
        if (finalXp <= 0)
        {
            return new XpGrantResult
            {
                TotalXpAfter = user.TotalXP,
                LevelAfter = user.Level
            };
        }

        var oldLevel = user.Level;
        user.TotalXP += finalXp;
        var newLevel = LevelProgression.GetLevelFromTotalXp(user.TotalXP);
        var levelUp = newLevel > oldLevel;
        user.Level = newLevel;

        _context.XpGrantLogs.Add(new XpGrantLog
        {
            UserId = userId,
            ActionType = request.ActionType,
            ModuleId = request.ModuleId,
            BaseXp = baseXp,
            Multiplier = multiplier,
            FinalXp = finalXp,
            ReasonSummary = reason,
            GrantedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        if (levelUp)
        {
            await CheckAndGrantLevelAchievementsAsync(userId);
            await _notificationService.SendAsync(
                userId,
                NotificationTypes.LevelUp,
                "Новый уровень!",
                $"Поздравляем! Вы достигли уровня {newLevel}.",
                "/profile");
        }

        return new XpGrantResult
        {
            BaseXp = baseXp,
            Multiplier = multiplier,
            FinalXp = finalXp,
            ReasonSummary = reason,
            TotalXpAfter = user.TotalXP,
            LevelAfter = user.Level,
            LevelUp = levelUp
        };
    }

    public async Task AddXpAsync(int userId, int xpAmount)
    {
        if (xpAmount <= 0) return;
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        var oldLevel = user.Level;
        user.TotalXP += xpAmount;
        user.Level = LevelProgression.GetLevelFromTotalXp(user.TotalXP);

        _context.XpGrantLogs.Add(new XpGrantLog
        {
            UserId = userId,
            ActionType = "LEGACY",
            BaseXp = xpAmount,
            Multiplier = 1m,
            FinalXp = xpAmount,
            ReasonSummary = "стандартная награда",
            GrantedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        if (user.Level > oldLevel)
            await CheckAndGrantLevelAchievementsAsync(userId);
    }

    public async Task GrantAchievementAsync(int userId, string conditionKey)
    {
        var achievement = await _context.Achievements
            .FirstOrDefaultAsync(a => a.ConditionKey == conditionKey);

        if (achievement == null) return;

        var alreadyHas = await _context.UserAchievements
            .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.AchievementId);

        if (!alreadyHas)
        {
            _context.UserAchievements.Add(new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.AchievementId,
                AwardedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            await _notificationService.SendAsync(
                userId,
                NotificationTypes.Achievement,
                "Новое достижение!",
                $"{achievement.IconName} {achievement.Title}: {achievement.Description}",
                "/profile");
        }
    }

    public async Task CheckAndGrantLevelAchievementsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        if (user.Level >= 2) await GrantAchievementAsync(userId, "LEVEL_2");
        if (user.Level >= 5) await GrantAchievementAsync(userId, "LEVEL_5");
    }
}
