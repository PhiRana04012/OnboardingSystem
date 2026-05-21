using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Services;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly AppDbContext _context;

    public NotificationsController(INotificationService notificationService, AppDbContext context)
    {
        _notificationService = notificationService;
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), 200)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetRecent(
        [FromQuery] int limit = 30,
        [FromQuery] bool unreadOnly = false)
    {
        var user = await this.GetCurrentUserAsync(_context);
        if (user == null) return Unauthorized();

        var items = await _notificationService.GetRecentAsync(user.UserId, limit, unreadOnly);
        return Ok(items);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), 200)]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount()
    {
        var user = await this.GetCurrentUserAsync(_context);
        if (user == null) return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(user.UserId);
        return Ok(new UnreadCountDto { Count = count });
    }

    [HttpPost("{id}/read")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var user = await this.GetCurrentUserAsync(_context);
        if (user == null) return Unauthorized();

        var ok = await _notificationService.MarkAsReadAsync(id, user.UserId);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var user = await this.GetCurrentUserAsync(_context);
        if (user == null) return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(user.UserId);
        return NoContent();
    }
}
