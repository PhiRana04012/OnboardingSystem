namespace OnboardingSystem.DTOs;

public class UserModuleProgressDto
{
    public int ProgressId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int AttemptsCount { get; set; }
    public decimal? BestScore { get; set; }
    public bool IsPassed { get; set; }
}

public class UserOnboardingProgressDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string OnboardingStatus { get; set; } = null!;
    public int TotalMandatoryModules { get; set; }
    public int CompletedMandatoryModules { get; set; }
    public int TotalModules { get; set; }
    public int CompletedModules { get; set; }
    public decimal ProgressPercentage { get; set; }
    public List<ModuleProgressSummaryDto> Modules { get; set; } = new();
}

public class ModuleProgressSummaryDto
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = null!;
    public bool IsMandatory { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int AttemptsCount { get; set; }
    public decimal? BestScore { get; set; }
    public bool IsPassed { get; set; }
}

public class MarkModuleAsReadDto
{
    public int ModuleId { get; set; }
}


