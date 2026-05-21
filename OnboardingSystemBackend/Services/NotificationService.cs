using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using OnboardingSystem.Hubs;

namespace OnboardingSystem.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<NotificationDto> SendAsync(
        int userId,
        string type,
        string title,
        string message,
        string? linkUrl = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            LinkUrl = linkUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = ToDto(notification);
        var unreadCount = await GetUnreadCountAsync(userId, cancellationToken);

        await _hubContext.Clients
            .Group(NotificationHub.UserGroup(userId))
            .SendAsync("ReceiveNotification", dto, unreadCount, cancellationToken);

        return dto;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetRecentAsync(
        int userId,
        int limit = 30,
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .AsNoTracking();

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return items.Select(ToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default) =>
        await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId, cancellationToken);

        if (notification == null || notification.IsRead)
            return notification != null;

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);

        var unreadCount = await GetUnreadCountAsync(userId, cancellationToken);
        await _hubContext.Clients
            .Group(NotificationHub.UserGroup(userId))
            .SendAsync("UnreadCountUpdated", unreadCount, cancellationToken);

        return true;
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
            return;

        foreach (var n in unread)
            n.IsRead = true;

        await _context.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients
            .Group(NotificationHub.UserGroup(userId))
            .SendAsync("UnreadCountUpdated", 0, cancellationToken);
    }

    private static NotificationDto ToDto(Notification n) => new()
    {
        NotificationId = n.NotificationId,
        Type = n.Type,
        Title = n.Title,
        Message = n.Message,
        LinkUrl = n.LinkUrl,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
