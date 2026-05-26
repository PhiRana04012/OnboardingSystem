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
    private readonly IDepartmentAnalyticsService _departmentAnalyticsService;
    private readonly AppDbContext _context;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IProgressAnalyticsService analyticsService,
        ILearningPathService learningPathService,
        IDepartmentAnalyticsService departmentAnalyticsService,
        AppDbContext context,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _learningPathService = learningPathService;
        _departmentAnalyticsService = departmentAnalyticsService;
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

    // ==================== МЕНЕДЖЕРСКАЯ АНАЛИТИКА ====================

    /// <summary>
    /// Получить полный дашборд аналитики для отдела
    /// Включает метрики, прогресс сотрудников и активность по дням
    /// </summary>
    /// <param name="departmentId">ID отдела</param>
    [HttpGet("department/{departmentId}/dashboard")]
    [ProducesResponseType(typeof(DepartmentAnalyticsDashboardDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<DepartmentAnalyticsDashboardDto>> GetDepartmentDashboard(int departmentId)
    {
        try
        {
            // Проверка доступа - пользователь может видеть данные только своего отдела
            var currentUser = await this.GetCurrentUserAsync(_context);
            if (currentUser != null && currentUser.DepartmentId != departmentId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to access department {DepartmentId} analytics",
                    currentUser.UserId,
                    departmentId);
                return Forbid();
            }

            var dashboard = await _departmentAnalyticsService.GetDepartmentDashboardAsync(departmentId);
            return Ok(dashboard);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Department {DepartmentId} not found", departmentId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading department dashboard for {DepartmentId}", departmentId);
            return StatusCode(500, new { message = "Ошибка при загрузке аналитики отдела" });
        }
    }

    /// <summary>
    /// Получить метрики отдела
    /// </summary>
    /// <param name="departmentId">ID отдела</param>
    [HttpGet("department/{departmentId}/metrics")]
    [ProducesResponseType(typeof(DepartmentMetricsDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<DepartmentMetricsDto>> GetDepartmentMetrics(int departmentId)
    {
        try
        {
            var currentUser = await this.GetCurrentUserAsync(_context);
            if (currentUser != null && currentUser.DepartmentId != departmentId)
            {
                return Forbid();
            }

            var metrics = await _departmentAnalyticsService.GetDepartmentMetricsAsync(departmentId);
            return Ok(metrics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Department {DepartmentId} not found", departmentId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics for department {DepartmentId}", departmentId);
            return StatusCode(500, new { message = "Ошибка при получении метрик" });
        }
    }

    /// <summary>
    /// Получить прогресс каждого сотрудника в отделе
    /// </summary>
    /// <param name="departmentId">ID отдела</param>
    [HttpGet("department/{departmentId}/employees")]
    [ProducesResponseType(typeof(List<EmployeeProgressDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<EmployeeProgressDto>>> GetDepartmentEmployeeProgress(int departmentId)
    {
        try
        {
            var currentUser = await this.GetCurrentUserAsync(_context);
            if (currentUser != null && currentUser.DepartmentId != departmentId)
            {
                return Forbid();
            }

            var progress = await _departmentAnalyticsService.GetEmployeeProgressAsync(departmentId);
            return Ok(progress);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Department {DepartmentId} not found", departmentId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee progress for department {DepartmentId}", departmentId);
            return StatusCode(500, new { message = "Ошибка при получении прогресса сотрудников" });
        }
    }

    /// <summary>
    /// Получить активность по дням недели для отдела
    /// </summary>
    /// <param name="departmentId">ID отдела</param>
    [HttpGet("department/{departmentId}/activity")]
    [ProducesResponseType(typeof(List<DailyActivityDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<DailyActivityDto>>> GetDepartmentDailyActivity(int departmentId)
    {
        try
        {
            var currentUser = await this.GetCurrentUserAsync(_context);
            if (currentUser != null && currentUser.DepartmentId != departmentId)
            {
                return Forbid();
            }

            var activity = await _departmentAnalyticsService.GetDailyActivityAsync(departmentId);
            return Ok(activity);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Department {DepartmentId} not found", departmentId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity for department {DepartmentId}", departmentId);
            return StatusCode(500, new { message = "Ошибка при получении активности" });
        }
    }
}
