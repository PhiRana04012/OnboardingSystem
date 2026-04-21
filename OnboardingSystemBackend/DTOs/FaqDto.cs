namespace OnboardingSystem.DTOs;

public class FaqDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class FaqCreateDto
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Category { get; set; } = "Общее";
    public int DisplayOrder { get; set; }
}
