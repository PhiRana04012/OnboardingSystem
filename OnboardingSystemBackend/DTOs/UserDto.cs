namespace OnboardingSystem.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string? ExternalId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = null!;
    public int? MentorId { get; set; }
    public string? MentorName { get; set; }
    public DateOnly HireDate { get; set; }
    public string OnboardingStatus { get; set; } = null!;
    public string? JobTitle { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CreateUserDto
{
    public string? ExternalId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int DepartmentId { get; set; }
    public int? MentorId { get; set; }
    public DateOnly HireDate { get; set; }
    public List<int> RoleIds { get; set; } = new();
}

public class UpdateUserDto
{
    public string? ExternalId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
    public int? MentorId { get; set; }
    public DateOnly? HireDate { get; set; }
    public string? OnboardingStatus { get; set; }
    public List<int>? RoleIds { get; set; }
}


