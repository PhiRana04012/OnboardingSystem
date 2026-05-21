using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using OnboardingSystem.Services;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TestAttemptsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TestAttemptsController> _logger;
    private readonly IEmailService _emailService;
    private readonly IGamificationService _gamificationService;
    private readonly INotificationService _notificationService;
    private readonly IAuthorizationService _authorizationService;

    public TestAttemptsController(
        AppDbContext context,
        ILogger<TestAttemptsController> logger,
        IEmailService emailService,
        IGamificationService gamificationService,
        INotificationService notificationService,
        IAuthorizationService authorizationService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
        _gamificationService = gamificationService;
        _notificationService = notificationService;
        _authorizationService = authorizationService;
    }


    /// <summary>
    /// Получить попытки прохождения теста пользователя
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<TestAttemptDto>), 200)]
    public async Task<ActionResult<List<TestAttemptDto>>> GetUserAttempts(int userId)
    {
        var attempts = await _context.TestAttempts
            .Include(t => t.User)
            .Include(t => t.Module)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.AttemptDate)
            .Select(t => new TestAttemptDto
            {
                AttemptId = t.AttemptId,
                UserId = t.UserId,
                UserName = t.User.FullName,
                ModuleId = t.ModuleId,
                ModuleTitle = t.Module.Title,
                AttemptDate = t.AttemptDate,
                AttemptNumber = t.AttemptNumber,
                Score = t.Score,
                IsPassed = t.IsPassed
            })
            .ToListAsync();

        return Ok(attempts);
    }

    /// <summary>
    /// Получить попытки прохождения теста по модулю
    /// </summary>
    [HttpGet("module/{moduleId}/user/{userId}")]
    [ProducesResponseType(typeof(List<TestAttemptDto>), 200)]
    public async Task<ActionResult<List<TestAttemptDto>>> GetModuleAttempts(int moduleId, int userId)
    {
        var attempts = await _context.TestAttempts
            .Include(t => t.User)
            .Include(t => t.Module)
            .Where(t => t.ModuleId == moduleId && t.UserId == userId)
            .OrderByDescending(t => t.AttemptDate)
            .Select(t => new TestAttemptDto
            {
                AttemptId = t.AttemptId,
                UserId = t.UserId,
                UserName = t.User.FullName,
                ModuleId = t.ModuleId,
                ModuleTitle = t.Module.Title,
                AttemptDate = t.AttemptDate,
                AttemptNumber = t.AttemptNumber,
                Score = t.Score,
                IsPassed = t.IsPassed
            })
            .ToListAsync();

        return Ok(attempts);
    }

    /// <summary>
    /// Отправить ответы на тест
    /// </summary>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(TestResultDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TestResultDto>> SubmitTest([FromBody] SubmitTestDto dto)
    {
        var module = await _context.Modules
            .Include(m => m.Questions)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(m => m.ModuleId == dto.ModuleId);

        if (module == null)
        {
            return NotFound("Модуль не найден");
        }

        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        // Проверяем количество попыток
        var existingAttempts = await _context.TestAttempts
            .Where(t => t.UserId == dto.UserId && t.ModuleId == dto.ModuleId)
            .CountAsync();

        if (existingAttempts >= module.MaxAttempts)
        {
            return BadRequest($"Превышено максимальное количество попыток ({module.MaxAttempts})");
        }

        // Получаем правильные ответы
        var correctAnswers = await _context.Questions
            .Where(q => q.ModuleId == dto.ModuleId)
            .Select(q => new
            {
                QuestionId = q.QuestionId,
                CorrectAnswerId = q.AnswerOptions.First(a => a.IsCorrect).AnswerId
            })
            .ToDictionaryAsync(x => x.QuestionId, x => x.CorrectAnswerId);

        // Проверяем ответы
        var totalQuestions = correctAnswers.Count;
        var correctCount = 0;
        var questionResults = new List<QuestionResultDto>();

        foreach (var answer in dto.Answers)
        {
            if (correctAnswers.TryGetValue(answer.QuestionId, out var correctAnswerId))
            {
                var isCorrect = answer.AnswerId == correctAnswerId;
                if (isCorrect) correctCount++;

                var question = await _context.Questions
                    .FirstOrDefaultAsync(q => q.QuestionId == answer.QuestionId);

                questionResults.Add(new QuestionResultDto
                {
                    QuestionId = answer.QuestionId,
                    QuestionText = question?.QuestionText ?? "",
                    SelectedAnswerId = answer.AnswerId,
                    CorrectAnswerId = correctAnswerId,
                    IsCorrect = isCorrect
                });
            }
        }

        var score = totalQuestions > 0 ? (decimal)correctCount / totalQuestions * 100 : 0;
        var isPassed = score >= module.PassingScore;
        var attemptNumber = existingAttempts + 1;

        decimal? previousBestScore = null;
        if (existingAttempts > 0)
        {
            previousBestScore = await _context.TestAttempts
                .Where(t => t.UserId == dto.UserId && t.ModuleId == dto.ModuleId)
                .MaxAsync(t => (decimal?)t.Score);
        }

        // Сохраняем попытку
        var attempt = new TestAttempt
        {
            UserId = dto.UserId,
            ModuleId = dto.ModuleId,
            AttemptDate = DateTime.UtcNow,
            AttemptNumber = attemptNumber,
            Score = score,
            IsPassed = isPassed
        };

        _context.TestAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        // Обновляем прогресс модуля
        var progress = await _context.UserModuleProgresses
            .FirstOrDefaultAsync(p => p.UserId == dto.UserId && p.ModuleId == dto.ModuleId);

        if (progress == null)
        {
            progress = new UserModuleProgress
            {
                UserId = dto.UserId,
                ModuleId = dto.ModuleId,
                Status = "В процессе",
                StartDate = DateTime.UtcNow
            };
            _context.UserModuleProgresses.Add(progress);
        }

        if (isPassed)
        {
            progress.Status = "Завершён";
            progress.CompletionDate = DateTime.UtcNow;
        }
        else if (attemptNumber >= module.MaxAttempts)
        {
            progress.Status = "Не сдан";
        }

        // Логируем действие
        var actionLog = new ActionLog
        {
            UserId = dto.UserId,
            ActionType = "Тест пройден",
            Timestamp = DateTime.UtcNow,
            Details = $"Модуль: {module.Title}, Попытка: {attemptNumber}, Балл: {score:F2}%, Статус: {(isPassed ? "Сдано" : "Не сдано")}"
        };
        _context.ActionLogs.Add(actionLog);

        await _context.SaveChangesAsync();
        
        // --- Логика успешного окончания онбординга ---
        if (isPassed)
        {
            await CheckAndCompleteOnboardingAsync(dto.UserId);
        }

        var gamificationRequest = new Services.Gamification.GamificationXpRequest
        {
            ActionType = isPassed
                ? Services.Gamification.GamificationActionTypes.TestPassed
                : Services.Gamification.GamificationActionTypes.TestFailed,
            ModuleId = dto.ModuleId,
            ModuleTitle = module.Title,
            IsMandatoryModule = module.IsMandatory,
            TestScore = score,
            AttemptNumber = attemptNumber,
            TestPassed = isPassed,
            PreviousBestScore = previousBestScore
        };
        var xpResult = await _gamificationService.ProcessEventAsync(dto.UserId, gamificationRequest);

        if (isPassed)
        {
            await _notificationService.SendAsync(
                dto.UserId,
                NotificationTypes.TestPassed,
                "Тест сдан",
                $"Модуль «{module.Title}»: {score:F0}%.",
                $"/module/{dto.ModuleId}");
        }
        else
        {
            await _notificationService.SendAsync(
                dto.UserId,
                NotificationTypes.TestFailed,
                "Тест не сдан",
                $"Модуль «{module.Title}»: {score:F0}%. Осталось попыток: {Math.Max(0, module.MaxAttempts - attemptNumber)}.",
                $"/module/{dto.ModuleId}/test");
        }

        var result = new TestResultDto
        {
            AttemptId = attempt.AttemptId,
            ModuleId = dto.ModuleId,
            ModuleTitle = module.Title,
            AttemptDate = attempt.AttemptDate,
            AttemptNumber = attemptNumber,
            TotalQuestions = totalQuestions,
            CorrectAnswers = correctCount,
            Score = score,
            IsPassed = isPassed,
            CanRetry = !isPassed && attemptNumber < module.MaxAttempts,
            RemainingAttempts = Math.Max(0, module.MaxAttempts - attemptNumber),
            XpAwarded = xpResult.FinalXp,
            XpReason = xpResult.FinalXp > 0 ? $"+{xpResult.FinalXp} XP — {xpResult.ReasonSummary}" : null,
            QuestionResults = questionResults
        };

        return Ok(result);
    }

    /// <summary>
    /// Получить результат попытки
    /// </summary>
    [HttpGet("{attemptId}")]
    [ProducesResponseType(typeof(TestAttemptDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TestAttemptDto>> GetAttempt(long attemptId)
    {
        var attempt = await _context.TestAttempts
            .Include(t => t.User)
            .Include(t => t.Module)
            .FirstOrDefaultAsync(t => t.AttemptId == attemptId);

        if (attempt == null)
        {
            return NotFound();
        }

        var attemptDto = new TestAttemptDto
        {
            AttemptId = attempt.AttemptId,
            UserId = attempt.UserId,
            UserName = attempt.User.FullName,
            ModuleId = attempt.ModuleId,
            ModuleTitle = attempt.Module.Title,
            AttemptDate = attempt.AttemptDate,
            AttemptNumber = attempt.AttemptNumber,
            Score = attempt.Score,
            IsPassed = attempt.IsPassed
        };

        return Ok(attemptDto);
    }

    /// <summary>
    /// Сбросить попытки теста для пользователя (доступно только Админам/HR)
    /// </summary>
    [HttpDelete("reset/user/{userId}/module/{moduleId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ResetAttempts(int userId, int moduleId)
    {
        // Получаем текущего пользователя
        var currentUser = await this.GetCurrentUserAsync(_context);
        
        // Проверяем авторизацию: только Админы и HR могут сбрасывать попытки
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var isAdmin = _authorizationService.IsAdmin(currentUser);
        var isHr = _authorizationService.IsHr(currentUser);

        if (!isAdmin && !isHr)
        {
            return Forbid();
        }

        // Проверяем, что целевой пользователь существует
        var targetUser = await _context.Users.FindAsync(userId);
        if (targetUser == null)
        {
            return NotFound(new { message = "Пользователь не найден" });
        }

        // Проверяем, что модуль существует
        var module = await _context.Modules.FindAsync(moduleId);
        if (module == null)
        {
            return NotFound(new { message = "Модуль не найден" });
        }

        var attempts = await _context.TestAttempts
            .Where(t => t.UserId == userId && t.ModuleId == moduleId)
            .ToListAsync();

        if (attempts.Any())
        {
            _context.TestAttempts.RemoveRange(attempts);
        }
        
        var progress = await _context.UserModuleProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == moduleId);
            
        if (progress != null)
        {
            progress.Status = "В процессе";
            progress.CompletionDate = null;
        }

        // Логируем с указанием кто выполнил действие
        var actionLog = new ActionLog
        {
            UserId = currentUser.UserId,
            ActionType = "Администратор сбросил попытки теста",
            Timestamp = DateTime.UtcNow,
            Details = $"Сброшены попытки для пользователя {targetUser.FullName} (ID: {userId}) по модулю {module.Title} (ID: {moduleId}). Сброшено попыток: {attempts.Count}"
        };
        _context.ActionLogs.Add(actionLog);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task CheckAndCompleteOnboardingAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Mentor)
            .FirstOrDefaultAsync(u => u.UserId == userId);
            
        if (user == null || user.OnboardingStatus == "Завершён") return;

        var userModules = await _context.Modules
            .Where(m =>
                (m.DepartmentId == null && !m.ModuleDepartments.Any()) ||
                m.DepartmentId == user.DepartmentId ||
                m.ModuleDepartments.Any(md => md.DepartmentId == user.DepartmentId))
            .ToListAsync();
            
        var mandatoryModules = userModules.Where(m => m.IsMandatory).Select(m => m.ModuleId).ToList();
        
        var progressList = await _context.UserModuleProgresses
            .Where(p => p.UserId == userId && mandatoryModules.Contains(p.ModuleId))
            .ToListAsync();
            
        var completedMandatoryCount = progressList.Count(p => p.Status == "Завершён");
        
        if (mandatoryModules.Count > 0 && completedMandatoryCount == mandatoryModules.Count)
        {
            user.OnboardingStatus = "Завершён";
            
            var actionLog = new ActionLog
            {
                UserId = userId,
                ActionType = "Онбординг завершен",
                Timestamp = DateTime.UtcNow,
                Details = "Сотрудник завершил все обязательные модули"
            };
            _context.ActionLogs.Add(actionLog);
            await _context.SaveChangesAsync();

            // Find HR emails (Users with HR role)
            var hrEmails = await _context.Users
                .Where(u => u.Roles.Any(r => r.RoleName == "HR-специалист"))
                .Select(u => u.Email)
                .ToListAsync();

            var hrEmail = hrEmails.FirstOrDefault() ?? "";
            var mentorEmail = user.Mentor?.Email ?? "";

            await _emailService.SendOnboardingCompletedEmailAsync(hrEmail, mentorEmail, user.FullName);
            await _gamificationService.GrantAchievementAsync(userId, "ONBOARDING_DONE");

            await _notificationService.SendAsync(
                userId,
                NotificationTypes.OnboardingCompleted,
                "Онбординг завершён!",
                "Поздравляем! Вы прошли все обязательные модули.",
                "/");

            if (user.MentorId.HasValue)
            {
                await _notificationService.SendAsync(
                    user.MentorId.Value,
                    NotificationTypes.OnboardingCompleted,
                    "Подопечный завершил онбординг",
                    $"{user.FullName} завершил программу онбординга.",
                    "/mentor");
            }
        }
    }
}

