namespace OnboardingSystem.DTOs;

public class ActionLogDto
{
    public long LogId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string ActionType { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}

public class CreateActionLogDto
{
    public int UserId { get; set; }
    public string ActionType { get; set; } = null!;
    public string? Details { get; set; }
}


