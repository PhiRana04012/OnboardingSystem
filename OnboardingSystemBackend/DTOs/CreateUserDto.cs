namespace OnboardingSystem.DTOs;

/// <summary>
/// DTO для создания нового пользователя
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// Внешний ID пользователя (из внешней системы)
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Полное имя пользователя
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email пользователя
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ID подразделения
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// ID наставника (опционально)
    /// </summary>
    public int? MentorId { get; set; }

    /// <summary>
    /// Дата найма
    /// </summary>
    public DateOnly HireDate { get; set; }

    /// <summary>
    /// ID должности
    /// </summary>
    public int? JobTitleId { get; set; }

    /// <summary>
    /// Пароль (опционально - если не передан, пользователь получит письмо с ссылкой для установки)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Список ID ролей для назначения пользователю
    /// </summary>
    public List<int> RoleIds { get; set; } = new List<int>();
}

/// <summary>
/// DTO для обновления данных пользователя
/// </summary>
public class UpdateUserDto
{
    public string? ExternalId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
    public int? MentorId { get; set; }
    public DateOnly? HireDate { get; set; }
    public string? OnboardingStatus { get; set; }
    public int? JobTitleId { get; set; }
    public string? TelegramTag { get; set; }
    public string? Bio { get; set; }
    public List<int>? RoleIds { get; set; }
}
