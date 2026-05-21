using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Services;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IProgressAnalyticsService _analyticsService;
    private readonly ILearningPathService _learningPathService;
    private readonly AppDbContext _context;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IProgressAnalyticsService analyticsService,
        ILearningPathService learningPathService,
        AppDbContext context,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _learningPathService = learningPathService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Аналитика + план обучения + следующий модуль (один запрос, один вызов AI)
    /// </summary>
    [HttpGet("dashboard/{userId}")]
    [ProducesResponseType(typeof(AnalyticsDashboardDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetDashboard(int userId)
    {
        try
        {
            var analytics = await _analyticsService.AnalyzeUserProgressAsync(userId);
            var learningPath = await _learningPathService.GenerateLearningPathAsync(
                userId,
                "balanced",
                analytics);
            var nextModule = learningPath.PlannedOrder.FirstOrDefault(m => !m.IsCompleted);

            return Ok(new AnalyticsDashboardDto
            {
                Analytics = analytics,
                LearningPath = learningPath,
                NextModule = nextModule
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading analytics dashboard for user {UserId}", userId);
            return StatusCode(500, new { message = "Ошибка при загрузке аналитики" });
        }
    }

    /// <summary>
    /// Получить AI-анализ прогресса обучения пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    [HttpGet("progress-analysis/{userId}")]
    [ProducesResponseType(typeof(AIAnalyticsDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<AIAnalyticsDto>> GetProgressAnalysis(int userId)
    {
        try
        {
            var analysis = await _analyticsService.AnalyzeUserProgressAsync(userId);
            return Ok(analysis);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing progress for user {UserId}", userId);
            return StatusCode(500, new { message = "Ошибка при анализе прогресса" });
        }
    }

    /// <summary>
    /// Получить персональный путь обучения
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="strategy">Стратегия: fast-track, balanced, deep-learning</param>
    [HttpGet("learning-path/{userId}")]
    [ProducesResponseType(typeof(LearningPathDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<LearningPathDto>> GetLearningPath(
        int userId,
        [FromQuery] string strategy = "balanced")
    {
        try
        {
            var path = await _learningPathService.GenerateLearningPathAsync(userId, strategy);
            return Ok(path);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating learning path for user {UserId}", userId);
            return StatusCode(500, new { message = "Ошибка при создании плана обучения" });
        }
    }

    /// <summary>
    /// Получить следующий рекомендованный модуль для прохождения
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    [HttpGet("next-module/{userId}")]
    [ProducesResponseType(typeof(PlannedModuleDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<PlannedModuleDto?>> GetNextModule(int userId)
    {
        try
        {
            var module = await _learningPathService.GetNextRecommendedModuleAsync(userId);
            
            if (module == null)
            {
                return Ok((PlannedModuleDto?)null);
            }

            return Ok(module);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next module for user {UserId}", userId);
            return StatusCode(500, new { message = "Ошибка при получении следующего модуля" });
        }
    }

    /// <summary>
    /// Получить слабые и сильные области пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    [HttpGet("areas/{userId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> GetWeakAndStrongAreas(int userId)
    {
        try
        {
            var (weak, strong) = await _analyticsService.GetWeakAndStrongAreasAsync(userId);
            return Ok(new { weakAreas = weak, strengths = strong });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting areas for user {UserId}", userId);
            return StatusCode(500, new { message = "Ошибка при получении информации" });
        }
    }
}
