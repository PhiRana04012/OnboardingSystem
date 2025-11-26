using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using OnboardingSystem.Services;

namespace OnboardingSystem.Controllers;


/// Контроллер для синхронизации с RIMS API

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RimsSyncController : ControllerBase
{
    private readonly IRimsIntegrationService _rimsService;
    private readonly AppDbContext _context;
    private readonly ILogger<RimsSyncController> _logger;

    public RimsSyncController(
        IRimsIntegrationService rimsService,
        AppDbContext context,
        ILogger<RimsSyncController> logger)
    {
        _rimsService = rimsService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Синхронизировать пользователя из RIMS по Uid
    /// </summary>
    [HttpPost("sync-user/{rimsUid}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<UserDto>> SyncUserByUid(string rimsUid, CancellationToken ct = default)
    {
        try
        {
            var user = await _rimsService.SyncUserFromRimsAsync(rimsUid, ct);

            await _context.Entry(user)
                .Reference(u => u.Department)
                .LoadAsync(ct);
            await _context.Entry(user)
                .Reference(u => u.Mentor)
                .LoadAsync(ct);
            await _context.Entry(user)
                .Collection(u => u.Roles)
                .LoadAsync(ct);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                ExternalId = user.ExternalId,
                FullName = user.FullName,
                Email = user.Email,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name ?? "Не указано",
                MentorId = user.MentorId,
                MentorName = user.Mentor != null ? user.Mentor.FullName : null,
                HireDate = user.HireDate,
                OnboardingStatus = user.OnboardingStatus,
                JobTitle = user.JobTitle,
                Roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>()
            };

            return Ok(userDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Пользователь не найден в RIMS: {Uid}", rimsUid);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при синхронизации пользователя из RIMS: {Uid}", rimsUid);
            return BadRequest(new { message = "Ошибка при синхронизации пользователя из RIMS", error = ex.Message });
        }
    }

    /// <summary>
    /// Синхронизировать пользователя из RIMS по Email
    /// </summary>
    [HttpPost("sync-by-email")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<UserDto>> SyncUserByEmail([FromBody] SyncByEmailRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email обязателен для заполнения" });
        }

        try
        {
            // Найти пользователя в RIMS по email
            var rimsPerson = await _rimsService.FindPersonByEmailAsync(request.Email, ct);
            if (rimsPerson == null)
            {
                return NotFound(new { message = $"Пользователь с email '{request.Email}' не найден в RIMS" });
            }

            // Синхронизировать по Uid
            var user = await _rimsService.SyncUserFromRimsAsync(rimsPerson.Uid, ct);

            await _context.Entry(user)
                .Reference(u => u.Department)
                .LoadAsync(ct);
            await _context.Entry(user)
                .Reference(u => u.Mentor)
                .LoadAsync(ct);
            await _context.Entry(user)
                .Collection(u => u.Roles)
                .LoadAsync(ct);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                ExternalId = user.ExternalId,
                FullName = user.FullName,
                Email = user.Email,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name ?? "Не указано",
                MentorId = user.MentorId,
                MentorName = user.Mentor != null ? user.Mentor.FullName : null,
                HireDate = user.HireDate,
                OnboardingStatus = user.OnboardingStatus,
                JobTitle = user.JobTitle,
                Roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>()
            };

            return Ok(userDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Ошибка при поиске пользователя в RIMS: {Email}", request.Email);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при синхронизации пользователя из RIMS: {Email}", request.Email);
            return BadRequest(new { message = "Ошибка при синхронизации пользователя из RIMS", error = ex.Message });
        }
    }
}

/// <summary>
/// Запрос на синхронизацию по email
/// </summary>
public class SyncByEmailRequest
{
    public string Email { get; set; } = string.Empty;
}

