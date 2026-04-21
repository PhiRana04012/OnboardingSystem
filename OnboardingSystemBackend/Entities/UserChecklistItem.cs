using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnboardingSystem.Entities;

public class UserChecklistItem
{
    [Key]
    public int UserChecklistItemId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ChecklistItemId { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    [ForeignKey("UserId")]
    [JsonIgnore]
    public virtual User User { get; set; } = null!;

    [ForeignKey("ChecklistItemId")]
    [JsonIgnore]
    public virtual ChecklistItem ChecklistItem { get; set; } = null!;
}
