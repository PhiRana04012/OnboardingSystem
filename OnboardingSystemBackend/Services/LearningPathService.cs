using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;

namespace OnboardingSystem.Services;

public class LearningPathService : ILearningPathService
{
    private readonly AppDbContext _context;
    private readonly IProgressAnalyticsService _analyticsService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LearningPathService> _logger;

    public LearningPathService(
        AppDbContext context,
        IProgressAnalyticsService analyticsService,
        IMemoryCache cache,
        ILogger<LearningPathService> logger)
    {
        _context = context;
        _analyticsService = analyticsService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<LearningPathDto> GenerateLearningPathAsync(
        int userId,
        string strategy = "balanced",
        AIAnalyticsDto? analytics = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = LearningPathCacheKey(userId, strategy);
        if (analytics == null && _cache.TryGetValue(cacheKey, out LearningPathDto? cached) && cached != null)
        {
            _logger.LogDebug("Returning cached learning path for user {UserId}", userId);
            return cached;
        }

        _logger.LogInformation(
            "Generating learning path for user {UserId} with strategy {Strategy}",
            userId,
            strategy);

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.JobTitle)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            throw new ArgumentException($"User {userId} not found");
        }

        analytics ??= await _analyticsService.AnalyzeUserProgressAsync(userId, cancellationToken);

        var allModules = await _context.Modules
            .AsNoTracking()
            .Where(m => m.IsMandatory || m.DepartmentId == user.DepartmentId || !m.ModuleDepartments.Any())
            .ToListAsync(cancellationToken);

        var attempts = await _context.TestAttempts
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .GroupBy(t => t.ModuleId)
            .Select(g => new { ModuleId = g.Key, IsPassed = g.Any(t => t.IsPassed) })
            .ToListAsync(cancellationToken);

        var completedModuleIds = attempts.Where(a => a.IsPassed).Select(a => a.ModuleId).ToHashSet();
        var remainingModules = allModules.Where(m => !completedModuleIds.Contains(m.ModuleId)).ToList();
        var plannedModules = ParseAndPrioritizeModules(remainingModules, analytics, strategy);
        var aiPlan = BuildLocalExplanation(user, remainingModules, analytics, strategy);

        var learningPath = new LearningPathDto
        {
            UserId = userId,
            PlannedOrder = plannedModules,
            Strategy = strategy,
            AIExplanation = aiPlan,
            CreatedAt = DateTime.UtcNow,
            EstimatedCompletionDays = CalculateEstimatedDays(plannedModules)
        };

        _cache.Set(cacheKey, learningPath, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "Learning path generated for user {UserId}. Modules: {Count}",
            userId,
            plannedModules.Count);

        return learningPath;
    }

    public async Task<PlannedModuleDto?> GetNextRecommendedModuleAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting next recommended module for user {UserId}", userId);

        var path = await GetUserLearningPathAsync(userId, cancellationToken)
            ?? await GenerateLearningPathAsync(userId, cancellationToken: cancellationToken);

        var nextModule = path.PlannedOrder.FirstOrDefault(m => !m.IsCompleted);

        if (nextModule != null)
        {
            _logger.LogInformation("Next module for user {UserId}: {ModuleId}", userId, nextModule.ModuleId);
        }

        return nextModule;
    }

    public Task<LearningPathDto?> GetUserLearningPathAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(LearningPathCacheKey(userId, "balanced"), out LearningPathDto? path);
        return Task.FromResult(path);
    }

    private static string LearningPathCacheKey(int userId, string strategy) =>
        $"learning-path:{userId}:{strategy}";

    private static string BuildLocalExplanation(
        Entities.User user,
        List<Entities.Module> remainingModules,
        AIAnalyticsDto analytics,
        string strategy)
    {
        var nextTitle = remainingModules.FirstOrDefault()?.Title ?? "модули онбординга";
        var weakHint = analytics.WeakAreas.Count > 0
            ? $"Сначала пройдите: {string.Join(", ", analytics.WeakAreas.Take(2).Select(w => w.ModuleTitle))}."
            : "Сильных отстающих модулей нет — двигайтесь по плану.";

        var strategyHint = strategy switch
        {
            "fast-track" => "Режим: быстрый трек — приоритет обязательным модулям.",
            "deep-learning" => "Режим: углублённое изучение всех тем.",
            _ => "Режим: сбалансированный порядок."
        };

        return $"План для {user.JobTitle?.Title ?? "сотрудника"}: {strategyHint} {weakHint} Далее: {nextTitle}.";
    }

    private static List<PlannedModuleDto> ParseAndPrioritizeModules(
        List<Entities.Module> modules,
        AIAnalyticsDto analytics,
        string strategy)
    {
        var planned = new List<PlannedModuleDto>();
        var dayCounter = 1;

        foreach (var weakArea in analytics.WeakAreas.OrderByDescending(w => w.FailedAttempts))
        {
            var module = modules.FirstOrDefault(m => m.ModuleId == weakArea.ModuleId);
            if (module != null)
            {
                planned.Add(new PlannedModuleDto
                {
                    ModuleId = module.ModuleId,
                    ModuleTitle = module.Title,
                    Priority = 10 - Math.Min(planned.Count, 5),
                    Reason = $"Критично для вашей роли. Текущий результат: {weakArea.AverageScore:F0}%",
                    EstimatedHours = 3,
                    DayNumber = dayCounter++,
                    IsCompleted = false
                });
            }
        }

        var remaining = modules.Where(m => planned.All(p => p.ModuleId != m.ModuleId)).ToList();
        var ordered = strategy == "fast-track"
            ? remaining.OrderByDescending(m => m.IsMandatory).ThenBy(m => m.Title)
            : remaining.OrderBy(m => m.Title);

        foreach (var module in ordered)
        {
            var priority = Math.Max(5 - planned.Count / 3, 1);
            planned.Add(new PlannedModuleDto
            {
                ModuleId = module.ModuleId,
                ModuleTitle = module.Title,
                Priority = priority,
                Reason = "Требуется для полного онбординга",
                EstimatedHours = 2,
                DayNumber = dayCounter++,
                IsCompleted = false
            });
        }

        return planned;
    }

    private static double CalculateEstimatedDays(List<PlannedModuleDto> modules)
    {
        var totalHours = modules.Sum(m => m.EstimatedHours);
        return Math.Ceiling(totalHours / 4.0);
    }
}
