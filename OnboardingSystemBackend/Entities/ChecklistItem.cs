using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnboardingSystem.Entities;

public class ChecklistItem
{
    [Key]
    public int ChecklistItemId { get; set; }

    [Required]
    public int ModuleId { get; set; }

    [Required]
    public string Text { get; set; } = null!;

    public bool IsRequired { get; set; } = true;
    
    public int OrderIndex { get; set; } = 0;

    [ForeignKey("ModuleId")]
    [JsonIgnore]
    public virtual Module Module { get; set; } = null!;
}
