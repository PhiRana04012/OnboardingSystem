namespace OnboardingSystem.DTOs;

public class AIAnalyticsDto
{
    public int UserId { get; set; }
    public List<WeakAreaDto> WeakAreas { get; set; } = new();
    public List<StrengthDto> Strengths { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public double EstimatedCompletionDays { get; set; }
    public string AIInsight { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class WeakAreaDto
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public int FailedAttempts { get; set; }
    public int TotalAttempts { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class StrengthDto
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public int SuccessfulAttempts { get; set; }
    public string Category { get; set; } = string.Empty;
}
