using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Services;

public interface IGamificationService
{
    Task AddXpAsync(int userId, int xpAmount);
    Task GrantAchievementAsync(int userId, string conditionKey);
    Task CheckAndGrantLevelAchievementsAsync(int userId);
}

public class GamificationService : IGamificationService
{
    private readonly AppDbContext _context;

    public GamificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddXpAsync(int userId, int xpAmount)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        user.TotalXP += xpAmount;

        // Simple leveling logic: Level = (TotalXP / 100) + 1
        var newLevel = (user.TotalXP / 100) + 1;
        if (newLevel > user.Level)
        {
            user.Level = newLevel;
            await CheckAndGrantLevelAchievementsAsync(userId);
        }

        await _context.SaveChangesAsync();
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
