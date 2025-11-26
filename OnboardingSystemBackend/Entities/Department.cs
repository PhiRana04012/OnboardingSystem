using System;
using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string Name { get; set; } = null!;

    public string? ExternalId { get; set; }

    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
