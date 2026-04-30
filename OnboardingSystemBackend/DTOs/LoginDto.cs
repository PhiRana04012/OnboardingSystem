namespace OnboardingSystem.DTOs;

/// <summary>
/// DTO для аутентификации пользователя
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Email пользователя
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO для результата аутентификации
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Успешна ли аутентификация
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// JWT токен
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Данные пользователя
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    public string? ErrorMessage { get; set; }
}
