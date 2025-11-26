using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Services;

/// <summary>
/// Интерфейс для интеграции с RIMS API
/// </summary>
public interface IRimsIntegrationService
{
    /// <summary>
    /// Поиск пользователя в RIMS по Uid
    /// </summary>
    Task<RimsPersonItem?> FindPersonByUidAsync(string uid, CancellationToken ct = default);

    /// <summary>
    /// Поиск пользователя в RIMS по Email
    /// </summary>
    Task<RimsPersonItem?> FindPersonByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Синхронизация: создание/обновление пользователя из RIMS
    /// </summary>
    Task<User> SyncUserFromRimsAsync(string rimsUid, CancellationToken ct = default);

    /// <summary>
    /// Получение или создание Department по названию из RIMS
    /// </summary>
    Task<Department> GetOrCreateDepartmentAsync(string rimsDepartmentName, CancellationToken ct = default);
}



