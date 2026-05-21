using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;

namespace OnboardingSystem.Services;

public class ProgressAnalyticsService : IProgressAnalyticsService
{
    private readonly AppDbContext _context;
    private readonly IAiMentorService _aiMentor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProgressAnalyticsService> _logger;

    public ProgressAnalyticsService(
        AppDbContext context,
        IAiMentorService aiMentor,
        IMemoryCache cache,
        ILogger<ProgressAnalyticsService> logger)
    {
        _context = context;
        _aiMentor = aiMentor;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AIAnalyticsDto> AnalyzeUserProgressAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"progress-analytics:{userId}";
        if (_cache.TryGetValue(cacheKey, out AIAnalyticsDto? cached) && cached != null)
        {
            _logger.LogDebug("Returning cached progress analysis for user {UserId}", userId);
            return cached;
        }

        _logger.LogInformation("Starting progress analysis for user {UserId}", userId);

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Department)
            .Include(u => u.JobTitle)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            throw new ArgumentException($"User {userId} not found");
        }

        var allModules = await _context.Modules.AsNoTracking().ToListAsync(cancellationToken);
        var attempts = await _context.TestAttempts
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);

        var moduleStats = allModules.Select(m =>
        {
            var moduleAttempts = attempts.Where(a => a.ModuleId == m.ModuleId).ToList();
            return new ModuleStat(
                m,
                moduleAttempts,
                moduleAttempts.Any() ? moduleAttempts.Average(a => (double)a.Score) : 0,
                moduleAttempts.Count(a => a.IsPassed),
                moduleAttempts.Count(a => !a.IsPassed),
                moduleAttempts.Count);
        }).ToList();

        var (weakAreas, strengths) = ExtractAreasFromStats(moduleStats);
        var analysisPrompt = BuildAnalysisPrompt(user, weakAreas, strengths);

        string aiAnalysis;
        try
        {
            aiAnalysis = await _aiMentor.AskAsync(
                analysisPrompt,
                includeKnowledgeContext: false,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI insight unavailable for user {UserId}", userId);
            aiAnalysis = BuildFallbackInsight(weakAreas, strengths);
        }

        var result = new AIAnalyticsDto
        {
            UserId = userId,
            WeakAreas = weakAreas,
            Strengths = strengths,
            Recommendations = ExtractRecommendations(aiAnalysis),
            EstimatedCompletionDays = CalculateEstimatedDays(weakAreas, strengths),
            AIInsight = aiAnalysis,
            GeneratedAt = DateTime.UtcNow
        };

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "Progress analysis completed for user {UserId}. Weak areas: {WeakCount}, Strengths: {StrengthCount}",
            userId,
            weakAreas.Count,
            strengths.Count);

        return result;
    }

    public async Task<(List<WeakAreaDto> Weak, List<StrengthDto> Strong)> GetWeakAndStrongAreasAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var attempts = await _context.TestAttempts
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);

        var allModules = await _context.Modules
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var moduleStats = allModules.Select(m =>
        {
            var moduleAttempts = attempts.Where(a => a.ModuleId == m.ModuleId).ToList();
            return new ModuleStat(
                m,
                moduleAttempts,
                moduleAttempts.Any() ? moduleAttempts.Average(a => (double)a.Score) : 0,
                moduleAttempts.Count(a => a.IsPassed),
                moduleAttempts.Count(a => !a.IsPassed),
                moduleAttempts.Count);
        }).ToList();

        return ExtractAreasFromStats(moduleStats);
    }

    private static (List<WeakAreaDto>, List<StrengthDto>) ExtractAreasFromStats(List<ModuleStat> moduleStats)
    {
        var weakAreas = new List<WeakAreaDto>();
        var strengths = new List<StrengthDto>();

        foreach (var stat in moduleStats)
        {
            if (stat.TotalAttempts == 0)
            {
                continue;
            }

            if (stat.AverageScore < 60)
            {
                weakAreas.Add(new WeakAreaDto
                {
                    ModuleId = stat.Module.ModuleId,
                    ModuleTitle = stat.Module.Title,
                    AverageScore = stat.AverageScore,
                    FailedAttempts = stat.FailCount,
                    TotalAttempts = stat.TotalAttempts,
                    Category = CategorizeModule(stat.Module.Title)
                });
            }
            else if (stat.AverageScore >= 80)
            {
                strengths.Add(new StrengthDto
                {
                    ModuleId = stat.Module.ModuleId,
                    ModuleTitle = stat.Module.Title,
                    AverageScore = stat.AverageScore,
                    SuccessfulAttempts = stat.PassCount,
                    Category = CategorizeModule(stat.Module.Title)
                });
            }
        }

        return (weakAreas.OrderByDescending(w => w.FailedAttempts).ToList(), strengths);
    }

    private static string BuildAnalysisPrompt(
        Entities.User user,
        List<WeakAreaDto> weakAreas,
        List<StrengthDto> strengths)
    {
        var weakList = string.Join(", ",
            weakAreas.Take(3).Select(w => $"{w.ModuleTitle} ({w.AverageScore:F0}%)"));

        var strongList = string.Join(", ",
            strengths.Take(3).Select(s => $"{s.ModuleTitle}"));

        return $@"Краткий анализ онбординга сотрудника:
Должность: {user.JobTitle?.Title ?? "?"}
Отдел: {user.Department?.Name ?? "?"}
Слабые: {(string.IsNullOrEmpty(weakList) ? "нет" : weakList)}
Сильные: {(string.IsNullOrEmpty(strongList) ? "нет" : strongList)}

Дай 1-2 предложения на русском о прогрессе и рекомендации.";
    }

    private static string BuildFallbackInsight(List<WeakAreaDto> weakAreas, List<StrengthDto> strengths)
    {
        if (weakAreas.Count == 0 && strengths.Count > 0)
        {
            return "Прогресс стабильный. Продолжайте проходить оставшиеся модули в рекомендованном порядке.";
        }

        if (weakAreas.Count > 0)
        {
            var titles = string.Join(", ", weakAreas.Take(2).Select(w => w.ModuleTitle));
            return $"Рекомендуем уделить внимание модулям: {titles}. Повторите материал и пройдите тест повторно.";
        }

        return "Продолжайте онбординг по плану. При вопросах обращайтесь к наставнику.";
    }

    private static List<string> ExtractRecommendations(string aiAnalysis)
    {
        var recommendations = new List<string>
        {
            "Сосредоточьтесь на модулях с низкими баллами",
            "Повторите материал сложных тем",
            "Используйте помощь наставника при необходимости"
        };

        if (aiAnalysis.Contains("безопасность", StringComparison.OrdinalIgnoreCase))
        {
            recommendations.Insert(0, "Критично улучшить знания о безопасности");
        }

        if (aiAnalysis.Contains("процесс", StringComparison.OrdinalIgnoreCase))
        {
            recommendations.Insert(0, "Важно разобраться в процессах");
        }

        return recommendations.Take(3).ToList();
    }

    private static double CalculateEstimatedDays(List<WeakAreaDto> weak, List<StrengthDto> strong)
    {
        var totalModules = weak.Count + strong.Count + 5;
        var avgDaysPerModule = weak.Any() ? 2.0 : 1.5;
        return Math.Ceiling(totalModules * avgDaysPerModule);
    }

    private static string CategorizeModule(string title)
    {
        if (title.Contains("безопас", StringComparison.OrdinalIgnoreCase))
        {
            return "Безопасность";
        }

        if (title.Contains("процесс", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("управлен", StringComparison.OrdinalIgnoreCase))
        {
            return "Процессы";
        }

        if (title.Contains("документ", StringComparison.OrdinalIgnoreCase))
        {
            return "Документооборот";
        }

        if (title.Contains("история", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("компан", StringComparison.OrdinalIgnoreCase))
        {
            return "Информация о компании";
        }

        return "Прочее";
    }

    private sealed record ModuleStat(
        Entities.Module Module,
        List<Entities.TestAttempt> Attempts,
        double AverageScore,
        int PassCount,
        int FailCount,
        int TotalAttempts);
}
