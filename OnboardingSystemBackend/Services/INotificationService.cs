using OnboardingSystem.DTOs;

namespace OnboardingSystem.Services;

public interface INotificationService
{
    Task<NotificationDto> SendAsync(
        int userId,
        string type,
        string title,
        string message,
        string? linkUrl = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> GetRecentAsync(
        int userId,
        int limit = 30,
        bool unreadOnly = false,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
}
