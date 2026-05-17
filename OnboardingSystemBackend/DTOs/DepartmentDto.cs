namespace OnboardingSystem.DTOs;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = null!;
    public int? HeadUserId { get; set; }
    public string? HeadUserName { get; set; }
    public int UserCount { get; set; }
    public int ModuleCount { get; set; }
}

public class CreateDepartmentDto
{
    public string Name { get; set; } = null!;
}

public class UpdateDepartmentDto
{
    public string? Name { get; set; }
}


