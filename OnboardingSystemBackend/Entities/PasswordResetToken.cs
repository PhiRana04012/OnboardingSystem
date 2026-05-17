using System;

namespace OnboardingSystem.Entities;

/// <summary>
/// Токен для установки/сброса пароля
/// </summary>
public class PasswordResetToken
{
    public int TokenId { get; set; }
    
    public int UserId { get; set; }
    
    /// <summary>
    /// Уникальный токен (GUID)
    /// </summary>
    public string Token { get; set; } = null!;
    
    /// <summary>
    /// Когда был создан токен
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Когда токен истекает (24 часа по умолчанию)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Был ли уже использован
    /// </summary>
    public bool IsUsed { get; set; } = false;
    
    /// <summary>
    /// Когда был использован
    /// </summary>
    public DateTime? UsedAt { get; set; }
    
    // Навигация
    public virtual User User { get; set; } = null!;
}
