using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Services;
using OnboardingSystem.Services.Gamification;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GamificationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IGamificationService _gamificationService;

    public GamificationController(AppDbContext context, IGamificationService gamificationService)
    {
        _context = context;
        _gamificationService = gamificationService;
    }

    /// <summary>
    /// Получить профиль геймификации пользователя
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(GamificationProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<GamificationProfileDto>> GetUserProfile(int userId)
    {
        var user = await _context.Users
            .Include(u => u.UserAchievements)
            .ThenInclude(ua => ua.Achievement)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        var allAchievements = await _context.Achievements.ToListAsync();
        var engagement = await _gamificationService.GetEngagementStateAsync(userId);
        var level = LevelProgression.GetLevelFromTotalXp(user.TotalXP);

        var recentGrants = await _context.XpGrantLogs
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.GrantedAt)
            .Take(5)
            .Select(x => new XpGrantDto
            {
                FinalXp = x.FinalXp,
                ActionType = x.ActionType,
                ReasonSummary = x.ReasonSummary,
                GrantedAt = x.GrantedAt
            })
            .ToListAsync();

        var dto = new GamificationProfileDto
        {
            UserId = user.UserId,
            TotalXP = user.TotalXP,
            Level = level,
            NextLevelXP = LevelProgression.ThresholdForLevel(level + 1),
            XpToNextLevel = LevelProgression.XpToNextLevel(user.TotalXP, level),
            LevelProgressPercent = LevelProgression.LevelProgressPercent(user.TotalXP, level),
            EngagementLabel = EngagementLabels.ToDisplayName(engagement.Cluster),
            RecentXpGrants = recentGrants
        };

        foreach (var ach in allAchievements)
        {
            var userAch = user.UserAchievements.FirstOrDefault(ua => ua.AchievementId == ach.AchievementId);
            var dt = new AchievementDto
            {
                AchievementId = ach.AchievementId,
                Title = ach.Title,
                Description = ach.Description,
                IconName = ach.IconName,
                IsEarned = userAch != null,
                AwardedAt = userAch?.AwardedAt
            };

            dto.AllAchievements.Add(dt);
            if (dt.IsEarned)
            {
                dto.EarnedAchievements.Add(dt);
            }
        }

        return Ok(dto);
    }
}
