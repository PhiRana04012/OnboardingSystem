using OnboardingSystem.Data;
using OnboardingSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnboardingSystem.Services;

/// <summary>
/// Сервис для работы с токенами установки/сброса пароля
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Создать токен для установки пароля новому пользователю
    /// </summary>
    Task<string> CreatePasswordSetupTokenAsync(int userId, TimeSpan expirationTime);
    
    /// <summary>
    /// Проверить валидность токена
    /// </summary>
    Task<(bool IsValid, int? UserId)> ValidateTokenAsync(string token);
    
    /// <summary>
    /// Установить пароль по токену
    /// </summary>
    Task<bool> SetPasswordWithTokenAsync(string token, string newPassword, PasswordHasher hasher);
    
    /// <summary>
    /// Отозвать все токены пользователя
    /// </summary>
    Task RevokeUserTokensAsync(int userId);
}

public class PasswordResetService : IPasswordResetService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(AppDbContext context, IEmailService emailService, ILogger<PasswordResetService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<string> CreatePasswordSetupTokenAsync(int userId, TimeSpan expirationTime)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new ArgumentException($"Пользователь с ID {userId} не найден");

        // Отозваем старые токены
        await RevokeUserTokensAsync(userId);

        var token = Guid.NewGuid().ToString("N"); // 32-символный хекс-код без дефисов
        
        var resetToken = new PasswordResetToken
        {
            UserId = userId,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expirationTime)
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Создан токен установки пароля для пользователя {user.Email}. Действителен до {resetToken.ExpiresAt:G}");

        return token;
    }

    public async Task<(bool IsValid, int? UserId)> ValidateTokenAsync(string token)
    {
        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);

        if (resetToken == null)
        {
            _logger.LogWarning($"❌ Токен не найден или уже использован: {token}");
            return (false, null);
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning($"❌ Токен истек: {token}");
            return (false, null);
        }

        return (true, resetToken.UserId);
    }

    public async Task<bool> SetPasswordWithTokenAsync(string token, string newPassword, PasswordHasher hasher)
    {
        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);

        if (resetToken == null || resetToken.ExpiresAt < DateTime.UtcNow)
            return false;

        // Устанавливаем пароль
        resetToken.User.PasswordHash = hasher.HashPassword(newPassword);
        
        // Отмечаем токен как использованный
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Пароль установлен для пользователя {resetToken.User.Email}");
        
        return true;
    }

    public async Task RevokeUserTokensAsync(int userId)
    {
        var tokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsUsed = true;
        }

        if (tokens.Any())
            await _context.SaveChangesAsync();
    }
}
