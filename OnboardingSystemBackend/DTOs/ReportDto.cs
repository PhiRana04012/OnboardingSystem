namespace OnboardingSystem.DTOs;

public class OnboardingProgressReportDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public string? MentorName { get; set; }
    public DateOnly HireDate { get; set; }
    public string OnboardingStatus { get; set; } = null!;
    public DateTime? OnboardingStartDate { get; set; }
    public DateTime? OnboardingCompletionDate { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int TotalMandatoryModules { get; set; }
    public int CompletedMandatoryModules { get; set; }
    public List<ModuleStatusDto> ModuleStatuses { get; set; } = new();
}

public class ModuleStatusDto
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

public class TestResultsReportDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = null!;
    public DateTime AttemptDate { get; set; }
    public int AttemptNumber { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
}

public class DepartmentReportDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = null!;
    public int TotalUsers { get; set; }
    public int UsersInProgress { get; set; }
    public int UsersCompleted { get; set; }
    public int UsersNotStarted { get; set; }
    public decimal AverageProgressPercentage { get; set; }
    public List<UserProgressSummaryDto> Users { get; set; } = new();
}

public class UserProgressSummaryDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string OnboardingStatus { get; set; } = null!;
    public decimal ProgressPercentage { get; set; }
    public DateTime? CompletionDate { get; set; }
}


