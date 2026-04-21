using System.ComponentModel.DataAnnotations;

namespace OnboardingSystem.Entities;

public class FaqEntry
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = "Общее";

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
