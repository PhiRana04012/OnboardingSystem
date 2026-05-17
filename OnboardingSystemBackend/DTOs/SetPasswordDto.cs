namespace OnboardingSystem.DTOs;

/// <summary>
/// DTO для установки пароля по токену
/// </summary>
public class SetPasswordDto
{
    /// <summary>
    /// Токен для установки пароля
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Новый пароль
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение пароля
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO для ответа при установке пароля
/// </summary>
public class SetPasswordResponseDto
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
}
