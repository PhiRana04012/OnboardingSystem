namespace OnboardingSystem.DTOs;

public class LearningPathDto
{
    public int UserId { get; set; }
    public List<PlannedModuleDto> PlannedOrder { get; set; } = new();
    public string Strategy { get; set; } = "balanced"; // fast-track, balanced, deep-learning
    public string AIExplanation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double EstimatedCompletionDays { get; set; }
}

public class PlannedModuleDto
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public int Priority { get; set; } // 1-10
    public string Reason { get; set; } = string.Empty; // Why this module in this order
    public int EstimatedHours { get; set; }
    public string DependsOn { get; set; } = string.Empty; // Module that should be completed first
    public bool IsCompleted { get; set; }
    public int DayNumber { get; set; } // Примерный день прохождения
}
