namespace OnboardingSystem.DTOs;

public class ChecklistItemDto
{
    public int ChecklistItemId { get; set; }
    public string Text { get; set; } = null!;
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ToggleChecklistDto
{
    public int UserId { get; set; }
    public int ChecklistItemId { get; set; }
    public bool IsCompleted { get; set; }
}

public class CreateChecklistItemDto
{
    public int ModuleId { get; set; }
    public string Text { get; set; } = null!;
    public bool IsRequired { get; set; }
    public int OrderIndex { get; set; }
}
