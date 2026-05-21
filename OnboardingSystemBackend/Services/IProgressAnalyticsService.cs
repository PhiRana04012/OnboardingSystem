using OnboardingSystem.DTOs;

namespace OnboardingSystem.Services;

public interface IProgressAnalyticsService
{
    /// <summary>
    /// Анализирует текущий прогресс пользователя через ИИ
    /// </summary>
    Task<AIAnalyticsDto> AnalyzeUserProgressAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить слабые и сильные области пользователя
    /// </summary>
    Task<(List<WeakAreaDto> Weak, List<StrengthDto> Strong)> GetWeakAndStrongAreasAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
