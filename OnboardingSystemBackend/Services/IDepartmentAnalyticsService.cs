using OnboardingSystem.DTOs;

namespace OnboardingSystem.Services;

/// <summary>
/// Сервис для получения аналитики по отделам
/// </summary>
public interface IDepartmentAnalyticsService
{
    /// <summary>
    /// Получить дашборд аналитики для отдела
    /// </summary>
    Task<DepartmentAnalyticsDashboardDto> GetDepartmentDashboardAsync(int departmentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить метрики по отделу
    /// </summary>
    Task<DepartmentMetricsDto> GetDepartmentMetricsAsync(int departmentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить прогресс каждого сотрудника в отделе
    /// </summary>
    Task<List<EmployeeProgressDto>> GetEmployeeProgressAsync(int departmentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить активность по дням недели
    /// </summary>
    Task<List<DailyActivityDto>> GetDailyActivityAsync(int departmentId, CancellationToken cancellationToken = default);
}
