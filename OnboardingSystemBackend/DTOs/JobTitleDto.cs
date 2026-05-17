namespace OnboardingSystem.DTOs;

public class JobTitleDto
{
    public int JobTitleId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
