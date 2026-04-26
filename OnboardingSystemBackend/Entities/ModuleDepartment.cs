namespace OnboardingSystem.Entities;

public class ModuleDepartment
{
    public int ModuleId { get; set; }
    public int DepartmentId { get; set; }

    public virtual Module Module { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
}
