using OnboardingSystem.DTOs;

namespace OnboardingSystem.Services;

public interface ILearningPathService
{
    /// <summary>
    /// Генерирует оптимальный путь обучения для пользователя
    /// </summary>
    Task<LearningPathDto> GenerateLearningPathAsync(
        int userId,
        string strategy = "balanced",
        AIAnalyticsDto? analytics = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить рекомендованный следующий модуль
    /// </summary>
    Task<PlannedModuleDto?> GetNextRecommendedModuleAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить сохранённый Learning Path пользователя
    /// </summary>
    Task<LearningPathDto?> GetUserLearningPathAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
