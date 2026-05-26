namespace OnboardingSystem.DTOs;

/// <summary>
/// Дашборд аналитики для менеджера отдела
/// </summary>
public class DepartmentAnalyticsDashboardDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Ключевые метрики отдела
    /// </summary>
    public DepartmentMetricsDto Metrics { get; set; } = new();
    
    /// <summary>
    /// Прогресс каждого сотрудника в отделе
    /// </summary>
    public List<EmployeeProgressDto> EmployeeProgress { get; set; } = new();
    
    /// <summary>
    /// Активность по дням недели
    /// </summary>
    public List<DailyActivityDto> ActivityByDay { get; set; } = new();
    
    /// <summary>
    /// Когда была сгенерирована статистика
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Метрики по отделу
/// </summary>
public class DepartmentMetricsDto
{
    /// <summary>
    /// Количество активных новичков (в процессе онбординга)
    /// </summary>
    public int ActiveNewcomers { get; set; }
    
    /// <summary>
    /// Количество завершивших онбординг
    /// </summary>
    public int CompletedOnboarding { get; set; }
    
    /// <summary>
    /// Средний XP в отделе
    /// </summary>
    public double AverageXP { get; set; }
    
    /// <summary>
    /// Средний уровень в отделе
    /// </summary>
    public double AverageLevel { get; set; }
    
    /// <summary>
    /// Процент завершения онбординга в отделе
    /// </summary>
    public double CompletionPercentage { get; set; }
    
    /// <summary>
    /// Среднее время адаптации (в днях)
    /// </summary>
    public double AverageOnboardingDays { get; set; }
    
    /// <summary>
    /// Общее количество сотрудников в отделе
    /// </summary>
    public int TotalEmployees { get; set; }
}

/// <summary>
/// Прогресс отдельного сотрудника
/// </summary>
public class EmployeeProgressDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Статус онбординга: active, completed, paused
    /// </summary>
    public string OnboardingStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Текущий уровень
    /// </summary>
    public int Level { get; set; }
    
    /// <summary>
    /// Текущий XP
    /// </summary>
    public int TotalXP { get; set; }
    
    /// <summary>
    /// Процент завершения онбординга
    /// </summary>
    public double CompletionPercentage { get; set; }
    
    /// <summary>
    /// Дни на онбординге
    /// </summary>
    public int DaysInOnboarding { get; set; }
    
    /// <summary>
    /// Дата найма
    /// </summary>
    public DateOnly HireDate { get; set; }
    
    /// <summary>
    /// ФИО наставника
    /// </summary>
    public string? MentorName { get; set; }
    
    /// <summary>
    /// Есть ли недавняя активность (последние 3 дня)
    /// </summary>
    public bool HasRecentActivity { get; set; }
}

/// <summary>
/// Активность по дням недели
/// </summary>
public class DailyActivityDto
{
    /// <summary>
    /// День недели (Monday, Tuesday, etc.)
    /// </summary>
    public string DayOfWeek { get; set; } = string.Empty;
    
    /// <summary>
    /// Количество активных пользователей в этот день
    /// </summary>
    public int ActiveUsers { get; set; }
    
    /// <summary>
    /// Количество выполненных тестов
    /// </summary>
    public int TestsCompleted { get; set; }
    
    /// <summary>
    /// Количество модулей завершено
    /// </summary>
    public int ModulesCompleted { get; set; }
    
    /// <summary>
    /// Средний набранный XP в этот день
    /// </summary>
    public double AverageXpGained { get; set; }
}
