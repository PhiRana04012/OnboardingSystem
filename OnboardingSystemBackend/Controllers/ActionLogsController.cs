using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ActionLogsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ActionLogsController> _logger;

    public ActionLogsController(AppDbContext context, ILogger<ActionLogsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить журнал действий пользователя
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<ActionLogDto>), 200)]
    public async Task<ActionResult<List<ActionLogDto>>> GetUserLogs(int userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = _context.ActionLogs
            .Include(l => l.User)
            .Where(l => l.UserId == userId)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(l => l.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.Timestamp <= endDate.Value);
        }

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new ActionLogDto
            {
                LogId = l.LogId,
                UserId = l.UserId,
                UserName = l.User.FullName,
                ActionType = l.ActionType,
                Timestamp = l.Timestamp,
                Details = l.Details
            })
            .ToListAsync();

        return Ok(logs);
    }

    /// <summary>
    /// Получить все записи журнала действий
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ActionLogDto>), 200)]
    public async Task<ActionResult<List<ActionLogDto>>> GetAllLogs([FromQuery] int? userId, [FromQuery] string? actionType, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = _context.ActionLogs
            .Include(l => l.User)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(l => l.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(actionType))
        {
            query = query.Where(l => l.ActionType.Contains(actionType));
        }

        if (startDate.HasValue)
        {
            query = query.Where(l => l.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.Timestamp <= endDate.Value);
        }

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new ActionLogDto
            {
                LogId = l.LogId,
                UserId = l.UserId,
                UserName = l.User.FullName,
                ActionType = l.ActionType,
                Timestamp = l.Timestamp,
                Details = l.Details
            })
            .ToListAsync();

        return Ok(logs);
    }

    /// <summary>
    /// Создать запись в журнале действий
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ActionLogDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ActionLogDto>> CreateLog([FromBody] CreateActionLogDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
        {
            return BadRequest("Пользователь не найден");
        }

        var log = new ActionLog
        {
            UserId = dto.UserId,
            ActionType = dto.ActionType,
            Timestamp = DateTime.UtcNow,
            Details = dto.Details
        };

        _context.ActionLogs.Add(log);
        await _context.SaveChangesAsync();

        await _context.Entry(log)
            .Reference(l => l.User)
            .LoadAsync();

        var logDto = new ActionLogDto
        {
            LogId = log.LogId,
            UserId = log.UserId,
            UserName = log.User.FullName,
            ActionType = log.ActionType,
            Timestamp = log.Timestamp,
            Details = log.Details
        };

        return CreatedAtAction(nameof(GetAllLogs), new { userId = log.UserId }, logDto);
    }
}


