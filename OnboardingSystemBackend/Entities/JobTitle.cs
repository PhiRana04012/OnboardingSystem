using System;
using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public partial class JobTitle
{
    public int JobTitleId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
