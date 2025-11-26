using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TestAttemptsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TestAttemptsController> _logger;

    public TestAttemptsController(AppDbContext context, ILogger<TestAttemptsController> logger)
    {
        _context = context;
        _logger = logger;
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
}

