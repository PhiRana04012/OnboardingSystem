using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProgressController> _logger;

    public ProgressController(AppDbContext context, ILogger<ProgressController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить прогресс онбординга пользователя
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(UserOnboardingProgressDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserOnboardingProgressDto>> GetUserProgress(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound();
        }

        // Получаем все модули для пользователя (общие + по подразделению)
        var userModules = await _context.Modules
            .Where(m => m.DepartmentId == null || m.DepartmentId == user.DepartmentId)
            .ToListAsync();

        var mandatoryModules = userModules.Where(m => m.IsMandatory).ToList();
        var totalMandatoryModules = mandatoryModules.Count;
        var totalModules = userModules.Count;

        // Получаем прогресс по модулям
        var progressList = await _context.UserModuleProgresses
            .Include(p => p.Module)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        // Получаем попытки тестов
        var attempts = await _context.TestAttempts
            .Where(t => t.UserId == userId)
            .GroupBy(t => t.ModuleId)
            .Select(g => new
            {
                ModuleId = g.Key,
                AttemptsCount = g.Count(),
                BestScore = g.Max(t => t.Score),
                IsPassed = g.Any(t => t.IsPassed)
            })
            .ToDictionaryAsync(x => x.ModuleId, x => x);

        var moduleSummaries = new List<ModuleProgressSummaryDto>();

        foreach (var module in userModules)
        {
            var progress = progressList.FirstOrDefault(p => p.ModuleId == module.ModuleId);
            var attemptInfo = attempts.GetValueOrDefault(module.ModuleId);

            moduleSummaries.Add(new ModuleProgressSummaryDto
            {
                ModuleId = module.ModuleId,
                ModuleTitle = module.Title,
                IsMandatory = module.IsMandatory,
                Status = progress?.Status ?? "Не начат",
                StartDate = progress?.StartDate,
                CompletionDate = progress?.CompletionDate,
                AttemptsCount = attemptInfo?.AttemptsCount ?? 0,
                BestScore = attemptInfo?.BestScore,
                IsPassed = attemptInfo?.IsPassed ?? false
            });
        }

        var completedMandatoryModules = moduleSummaries
            .Count(m => m.IsMandatory && m.Status == "Завершён");

        var completedModules = moduleSummaries
            .Count(m => m.Status == "Завершён");

        var progressPercentage = totalMandatoryModules > 0
            ? (decimal)completedMandatoryModules / totalMandatoryModules * 100
            : 0;

        var result = new UserOnboardingProgressDto
        {
            UserId = user.UserId,
            UserName = user.FullName,
            Email = user.Email,
            OnboardingStatus = user.OnboardingStatus,
            TotalMandatoryModules = totalMandatoryModules,
            CompletedMandatoryModules = completedMandatoryModules,
            TotalModules = totalModules,
            CompletedModules = completedModules,
            ProgressPercentage = progressPercentage,
            Modules = moduleSummaries
        };

        return Ok(result);
    }

    /// <summary>
    /// Получить прогресс по модулю
    /// </summary>
    [HttpGet("user/{userId}/module/{moduleId}")]
    [ProducesResponseType(typeof(UserModuleProgressDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserModuleProgressDto>> GetModuleProgress(int userId, int moduleId)
    {
        var progress = await _context.UserModuleProgresses
            .Include(p => p.User)
            .Include(p => p.Module)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == moduleId);

        if (progress == null)
        {
            return NotFound();
        }

        var attempts = await _context.TestAttempts
            .Where(t => t.UserId == userId && t.ModuleId == moduleId)
            .ToListAsync();

        var progressDto = new UserModuleProgressDto
        {
            ProgressId = progress.ProgressId,
            UserId = progress.UserId,
            UserName = progress.User.FullName,
            ModuleId = progress.ModuleId,
            ModuleTitle = progress.Module.Title,
            Status = progress.Status,
            StartDate = progress.StartDate,
            CompletionDate = progress.CompletionDate,
            AttemptsCount = attempts.Count,
            BestScore = attempts.Any() ? attempts.Max(t => t.Score) : null,
            IsPassed = attempts.Any(t => t.IsPassed)
        };

        return Ok(progressDto);
    }

    /// <summary>
    /// Отметить модуль как прочитанный
    /// </summary>
    [HttpPost("user/{userId}/mark-read")]
    [ProducesResponseType(typeof(UserModuleProgressDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<UserModuleProgressDto>> MarkModuleAsRead(int userId, [FromBody] MarkModuleAsReadDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        var module = await _context.Modules.FindAsync(dto.ModuleId);
        if (module == null)
        {
            return NotFound("Модуль не найден");
        }

        var progress = await _context.UserModuleProgresses
            .Include(p => p.User)
            .Include(p => p.Module)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == dto.ModuleId);

        if (progress == null)
        {
            progress = new UserModuleProgress
            {
                UserId = userId,
                ModuleId = dto.ModuleId,
                Status = "Завершён",
                StartDate = DateTime.UtcNow,
                CompletionDate = DateTime.UtcNow
            };
            _context.UserModuleProgresses.Add(progress);
        }
        else
        {
            progress.Status = "Завершён";
            if (progress.StartDate == null)
            {
                progress.StartDate = DateTime.UtcNow;
            }
            progress.CompletionDate = DateTime.UtcNow;
        }

        // Логируем действие
        var actionLog = new ActionLog
        {
            UserId = userId,
            ActionType = "Модуль прочитан",
            Timestamp = DateTime.UtcNow,
            Details = $"Модуль: {module.Title}"
        };
        _context.ActionLogs.Add(actionLog);

        await _context.SaveChangesAsync();

        await _context.Entry(progress)
            .Reference(p => p.User)
            .LoadAsync();
        await _context.Entry(progress)
            .Reference(p => p.Module)
            .LoadAsync();

        var attempts = await _context.TestAttempts
            .Where(t => t.UserId == userId && t.ModuleId == dto.ModuleId)
            .ToListAsync();

        var progressDto = new UserModuleProgressDto
        {
            ProgressId = progress.ProgressId,
            UserId = progress.UserId,
            UserName = progress.User.FullName,
            ModuleId = progress.ModuleId,
            ModuleTitle = progress.Module.Title,
            Status = progress.Status,
            StartDate = progress.StartDate,
            CompletionDate = progress.CompletionDate,
            AttemptsCount = attempts.Count,
            BestScore = attempts.Any() ? attempts.Max(t => t.Score) : null,
            IsPassed = attempts.Any(t => t.IsPassed)
        };

        return Ok(progressDto);
    }

    /// <summary>
    /// Получить список всех прогрессов
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserModuleProgressDto>), 200)]
    public async Task<ActionResult<List<UserModuleProgressDto>>> GetAllProgress([FromQuery] int? userId, [FromQuery] int? moduleId)
    {
        var query = _context.UserModuleProgresses
            .Include(p => p.User)
            .Include(p => p.Module)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (moduleId.HasValue)
        {
            query = query.Where(p => p.ModuleId == moduleId.Value);
        }

        var progressList = await query.ToListAsync();

        var userIds = progressList.Select(p => p.UserId).Distinct().ToList();
        var moduleIds = progressList.Select(p => p.ModuleId).Distinct().ToList();

        var attempts = await _context.TestAttempts
            .Where(t => userIds.Contains(t.UserId) && moduleIds.Contains(t.ModuleId))
            .GroupBy(t => new { t.UserId, t.ModuleId })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.ModuleId,
                AttemptsCount = g.Count(),
                BestScore = g.Max(t => t.Score),
                IsPassed = g.Any(t => t.IsPassed)
            })
            .ToDictionaryAsync(x => new { x.UserId, x.ModuleId }, x => x);

        var result = progressList.Select(p =>
        {
            var attemptInfo = attempts.GetValueOrDefault(new { p.UserId, p.ModuleId });
            return new UserModuleProgressDto
            {
                ProgressId = p.ProgressId,
                UserId = p.UserId,
                UserName = p.User.FullName,
                ModuleId = p.ModuleId,
                ModuleTitle = p.Module.Title,
                Status = p.Status,
                StartDate = p.StartDate,
                CompletionDate = p.CompletionDate,
                AttemptsCount = attemptInfo?.AttemptsCount ?? 0,
                BestScore = attemptInfo?.BestScore,
                IsPassed = attemptInfo?.IsPassed ?? false
            };
        }).ToList();

        return Ok(result);
    }
}


