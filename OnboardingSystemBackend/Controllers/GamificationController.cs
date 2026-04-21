using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GamificationController : ControllerBase
{
    private readonly AppDbContext _context;

    public GamificationController(AppDbContext context)
    {
        _context = context;
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

        var dto = new GamificationProfileDto
        {
            UserId = user.UserId,
            TotalXP = user.TotalXP,
            Level = user.Level,
            NextLevelXP = user.Level * 100, // simple calc: level 1 -> 100, level 2 -> 200 max.
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
