using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChecklistsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ChecklistsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить чек-лист модуля для конкретного пользователя
    /// </summary>
    [HttpGet("module/{moduleId}/user/{userId}")]
    public async Task<ActionResult<List<ChecklistItemDto>>> GetModuleChecklist(int moduleId, int userId)
    {
        var items = await _context.ChecklistItems
            .Where(i => i.ModuleId == moduleId)
            .OrderBy(i => i.OrderIndex)
            .Select(i => new ChecklistItemDto
            {
                ChecklistItemId = i.ChecklistItemId,
                Text = i.Text,
                IsRequired = i.IsRequired,
                IsCompleted = _context.UserChecklistItems
                    .Any(ui => ui.ChecklistItemId == i.ChecklistItemId && ui.UserId == userId && ui.IsCompleted),
                CompletedAt = _context.UserChecklistItems
                    .Where(ui => ui.ChecklistItemId == i.ChecklistItemId && ui.UserId == userId)
                    .Select(ui => ui.CompletedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Отметить пункт чек-листа
    /// </summary>
    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleItem([FromBody] ToggleChecklistDto dto)
    {
        var userChecklistItem = await _context.UserChecklistItems
            .FirstOrDefaultAsync(ui => ui.UserId == dto.UserId && ui.ChecklistItemId == dto.ChecklistItemId);

        if (userChecklistItem == null)
        {
            userChecklistItem = new UserChecklistItem
            {
                UserId = dto.UserId,
                ChecklistItemId = dto.ChecklistItemId,
                IsCompleted = dto.IsCompleted,
                CompletedAt = dto.IsCompleted ? DateTime.Now : null
            };
            _context.UserChecklistItems.Add(userChecklistItem);
        }
        else
        {
            userChecklistItem.IsCompleted = dto.IsCompleted;
            userChecklistItem.CompletedAt = dto.IsCompleted ? DateTime.Now : null;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Создать новый пункт чек-листа
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ChecklistItemDto>> CreateItem([FromBody] CreateChecklistItemDto dto)
    {
        var item = new ChecklistItem
        {
            ModuleId = dto.ModuleId,
            Text = dto.Text,
            IsRequired = dto.IsRequired,
            OrderIndex = dto.OrderIndex
        };

        _context.ChecklistItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetModuleChecklist), new { moduleId = item.ModuleId, userId = 0 }, new ChecklistItemDto
        {
            ChecklistItemId = item.ChecklistItemId,
            Text = item.Text,
            IsRequired = item.IsRequired,
            IsCompleted = false
        });
    }

    /// <summary>
    /// Обновить пункт чек-листа
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] CreateChecklistItemDto dto)
    {
        var item = await _context.ChecklistItems.FindAsync(id);
        if (item == null) return NotFound();

        item.Text = dto.Text;
        item.IsRequired = dto.IsRequired;
        item.OrderIndex = dto.OrderIndex;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Удалить пункт чек-листа
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await _context.ChecklistItems.FindAsync(id);
        if (item == null) return NotFound();

        // Также удаляем прогресс пользователей по этому пункту
        var userProgress = _context.UserChecklistItems.Where(ui => ui.ChecklistItemId == id);
        _context.UserChecklistItems.RemoveRange(userProgress);

        _context.ChecklistItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
