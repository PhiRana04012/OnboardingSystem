namespace OnboardingSystem.DTOs;

public class ModuleDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public bool IsMandatory { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; }
    public int QuestionCount { get; set; }
}

public class CreateModuleDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public bool IsMandatory { get; set; } = true;
    public int? DepartmentId { get; set; }
    public int PassingScore { get; set; } = 80;
    public int MaxAttempts { get; set; } = 3;
}

public class UpdateModuleDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public bool? IsMandatory { get; set; }
    public int? DepartmentId { get; set; }
    public int? PassingScore { get; set; }
    public int? MaxAttempts { get; set; }
}


