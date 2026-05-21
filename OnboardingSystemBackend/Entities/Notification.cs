namespace OnboardingSystem.Entities;

public class Notification
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
