using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;

namespace OnboardingSystem.Services;

/// <summary>
/// Сервис для получения аналитики по отделам
/// </summary>
public class DepartmentAnalyticsService : IDepartmentAnalyticsService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartmentAnalyticsService> _logger;

    public DepartmentAnalyticsService(AppDbContext context, ILogger<DepartmentAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить полный дашборд аналитики для отдела
    /// </summary>
    public async Task<DepartmentAnalyticsDashboardDto> GetDepartmentDashboardAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading department analytics dashboard for department {DepartmentId}", departmentId);

        var department = await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId, cancellationToken);

        if (department == null)
        {
            throw new ArgumentException($"Department {departmentId} not found");
        }

        var metrics = await GetDepartmentMetricsAsync(departmentId, cancellationToken);
        var employeeProgress = await GetEmployeeProgressAsync(departmentId, cancellationToken);
        var dailyActivity = await GetDailyActivityAsync(departmentId, cancellationToken);

        return new DepartmentAnalyticsDashboardDto
        {
            DepartmentId = departmentId,
            DepartmentName = department.Name,
            Metrics = metrics,
            EmployeeProgress = employeeProgress,
            ActivityByDay = dailyActivity,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Получить метрики по отделу
    /// </summary>
    public async Task<DepartmentMetricsDto> GetDepartmentMetricsAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calculating metrics for department {DepartmentId}", departmentId);

        var employees = await _context.Users
            .AsNoTracking()
            .Where(u => u.DepartmentId == departmentId && u.IsActive)
            .ToListAsync(cancellationToken);

        if (employees.Count == 0)
        {
            return new DepartmentMetricsDto
            {
                TotalEmployees = 0,
                ActiveNewcomers = 0,
                CompletedOnboarding = 0,
                AverageXP = 0,
                AverageLevel = 0,
                CompletionPercentage = 0,
                AverageOnboardingDays = 0
            };
        }

        var totalModules = await _context.Modules
            .AsNoTracking()
            .Where(m => 
                (m.DepartmentId == null && !m.ModuleDepartments.Any()) ||
                m.DepartmentId == departmentId ||
                m.ModuleDepartments.Any(md => md.DepartmentId == departmentId))
            .CountAsync(cancellationToken);

        // Активные новички (статус = "active")
        var activeNewcomers = employees.Count(u => u.OnboardingStatus == "active");
        
        // Завершившие онбординг (статус = "completed")
        var completedOnboarding = employees.Count(u => u.OnboardingStatus == "completed");

        // Средний XP и Level
        var averageXP = employees.Average(u => u.TotalXP);
        var averageLevel = employees.Average(u => u.Level);

        // Процент завершения - из userModuleProgress
        var totalProgress = await _context.UserModuleProgresses
            .AsNoTracking()
            .Where(p => employees.Select(e => e.UserId).Contains(p.UserId))
            .CountAsync(cancellationToken);

        var completedProgress = await _context.UserModuleProgresses
            .AsNoTracking()
            .Where(p => 
                employees.Select(e => e.UserId).Contains(p.UserId) &&
                p.Status == "completed")
            .CountAsync(cancellationToken);

        var completionPercentage = totalProgress > 0 ? (double)completedProgress / totalProgress * 100 : 0;

        // Среднее время адаптации
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var averageOnboardingDays = employees.Average(u => 
            Math.Max(0, (today.DayNumber - u.HireDate.DayNumber)));

        return new DepartmentMetricsDto
        {
            ActiveNewcomers = activeNewcomers,
            CompletedOnboarding = completedOnboarding,
            AverageXP = Math.Round(averageXP, 2),
            AverageLevel = Math.Round(averageLevel, 2),
            CompletionPercentage = Math.Round(completionPercentage, 2),
            AverageOnboardingDays = Math.Round(averageOnboardingDays, 2),
            TotalEmployees = employees.Count
        };
    }

    /// <summary>
    /// Получить прогресс каждого сотрудника в отделе
    /// </summary>
    public async Task<List<EmployeeProgressDto>> GetEmployeeProgressAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading employee progress for department {DepartmentId}", departmentId);

        var employees = await _context.Users
            .AsNoTracking()
            .Include(u => u.Mentor)
            .Where(u => u.DepartmentId == departmentId && u.IsActive)
            .ToListAsync(cancellationToken);

        var employeeIds = employees.Select(e => e.UserId).ToList();

        // Получить прогресс всех сотрудников
        var moduleProgress = await _context.UserModuleProgresses
            .AsNoTracking()
            .Where(p => employeeIds.Contains(p.UserId))
            .ToListAsync(cancellationToken);

        // Получить недавнюю активность (последние 3 дня)
        var threeDaysAgo = DateTime.UtcNow.AddDays(-3);
        var recentActivity = await _context.ActionLogs
            .AsNoTracking()
            .Where(a => 
                employeeIds.Contains(a.UserId) &&
                a.Timestamp >= threeDaysAgo)
            .Select(a => a.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return employees.Select(user =>
        {
            var userProgress = moduleProgress.Where(p => p.UserId == user.UserId).ToList();
            var totalCompletion = userProgress.Count > 0 
                ? userProgress.Count(p => p.Status == "completed") * 100.0 / userProgress.Count 
                : 0;

            return new EmployeeProgressDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                OnboardingStatus = user.OnboardingStatus,
                Level = user.Level,
                TotalXP = user.TotalXP,
                CompletionPercentage = Math.Round(totalCompletion, 2),
                DaysInOnboarding = Math.Max(0, today.DayNumber - user.HireDate.DayNumber),
                HireDate = user.HireDate,
                MentorName = user.Mentor?.FullName,
                HasRecentActivity = recentActivity.Contains(user.UserId)
            };
        }).OrderByDescending(e => e.TotalXP).ToList();
    }

    /// <summary>
    /// Получить активность по дням недели (за последние 4 недели)
    /// </summary>
    public async Task<List<DailyActivityDto>> GetDailyActivityAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading daily activity for department {DepartmentId}", departmentId);

        var employeeIds = await _context.Users
            .AsNoTracking()
            .Where(u => u.DepartmentId == departmentId && u.IsActive)
            .Select(u => u.UserId)
            .ToListAsync(cancellationToken);

        if (employeeIds.Count == 0)
        {
            return GetEmptyDailyActivity();
        }

        var fourWeeksAgo = DateTime.UtcNow.AddDays(-28);

        // Получить активность за последние 4 недели
        var actionLogs = await _context.ActionLogs
            .AsNoTracking()
            .Where(a => 
                employeeIds.Contains(a.UserId) &&
                a.Timestamp >= fourWeeksAgo)
            .ToListAsync(cancellationToken);

        // Получить завершенные тесты
        var testAttempts = await _context.TestAttempts
            .AsNoTracking()
            .Where(t => 
                employeeIds.Contains(t.UserId) &&
                t.AttemptDate >= fourWeeksAgo.Date)
            .ToListAsync(cancellationToken);

        // Получить завершенные модули
        var moduleProgress = await _context.UserModuleProgresses
            .AsNoTracking()
            .Where(p => 
                employeeIds.Contains(p.UserId) &&
                p.Status == "completed")
            .ToListAsync(cancellationToken);

        // Получить гранты XP
        var xpLogs = await _context.XpGrantLogs
            .AsNoTracking()
            .Where(x => 
                employeeIds.Contains(x.UserId) &&
                x.GrantedAt >= fourWeeksAgo)
            .ToListAsync(cancellationToken);

        // Группировать по дням недели
        var dayGroups = new Dictionary<string, (HashSet<int> Users, int Tests, int Modules, double XP)>
        {
            ["Monday"] = (new HashSet<int>(), 0, 0, 0),
            ["Tuesday"] = (new HashSet<int>(), 0, 0, 0),
            ["Wednesday"] = (new HashSet<int>(), 0, 0, 0),
            ["Thursday"] = (new HashSet<int>(), 0, 0, 0),
            ["Friday"] = (new HashSet<int>(), 0, 0, 0),
            ["Saturday"] = (new HashSet<int>(), 0, 0, 0),
            ["Sunday"] = (new HashSet<int>(), 0, 0, 0),
        };

        // Обработать логи активности
        foreach (var log in actionLogs)
        {
            var dayName = log.Timestamp.DayOfWeek.ToString();
            if (dayGroups.TryGetValue(dayName, out var group))
            {
                group.Users.Add(log.UserId);
            }
        }

        // Обработать попытки тестов
        foreach (var test in testAttempts)
        {
            var dayName = test.AttemptDate.DayOfWeek.ToString();
            if (dayGroups.TryGetValue(dayName, out var group))
            {
                group.Tests++;
                group.Users.Add(test.UserId);
            }
        }

        // Обработать завершенные модули
        foreach (var progress in moduleProgress.Where(p => p.CompletionDate.HasValue))
        {
            var dayName = progress.CompletionDate.Value.DayOfWeek.ToString();
            if (dayGroups.TryGetValue(dayName, out var group))
            {
                group.Modules++;
            }
        }

        // Обработать XP гранты
        foreach (var xp in xpLogs)
        {
            var dayName = xp.GrantedAt.DayOfWeek.ToString();
            if (dayGroups.TryGetValue(dayName, out var group))
            {
                group.XP += xp.FinalXp;
            }
        }

        // Создать результат
        var result = dayGroups
            .OrderBy(kvp => GetDayOfWeekNumber(kvp.Key))
            .Select(kvp => new DailyActivityDto
            {
                DayOfWeek = kvp.Key,
                ActiveUsers = kvp.Value.Users.Count,
                TestsCompleted = kvp.Value.Tests,
                ModulesCompleted = kvp.Value.Modules,
                AverageXpGained = kvp.Value.Users.Count > 0 
                    ? Math.Round(kvp.Value.XP / kvp.Value.Users.Count, 2)
                    : 0
            })
            .ToList();

        return result;
    }

    /// <summary>
    /// Получить пустую неделю активности
    /// </summary>
    private static List<DailyActivityDto> GetEmptyDailyActivity()
    {
        return new[]
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        }
        .Select(day => new DailyActivityDto
        {
            DayOfWeek = day,
            ActiveUsers = 0,
            TestsCompleted = 0,
            ModulesCompleted = 0,
            AverageXpGained = 0
        })
        .ToList();
    }

    /// <summary>
    /// Получить номер дня недели для сортировки
    /// </summary>
    private static int GetDayOfWeekNumber(string dayName)
    {
        return dayName switch
        {
            "Monday" => 1,
            "Tuesday" => 2,
            "Wednesday" => 3,
            "Thursday" => 4,
            "Friday" => 5,
            "Saturday" => 6,
            "Sunday" => 0,
            _ => 0
        };
    }
}
