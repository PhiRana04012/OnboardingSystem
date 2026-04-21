using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FaqController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<FaqController> _logger;

    public FaqController(AppDbContext context, ILogger<FaqController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить все вопросы FAQ
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FaqDto>), 200)]
    public async Task<ActionResult<List<FaqDto>>> GetFaq()
    {
        var faq = await _context.FaqEntries
            .OrderBy(f => f.DisplayOrder)
            .ThenByDescending(f => f.CreatedAt)
            .Select(f => new FaqDto
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer,
                Category = f.Category,
                DisplayOrder = f.DisplayOrder
            })
            .ToListAsync();

        return Ok(faq);
    }

    /// <summary>
    /// Создать новый вопрос FAQ (только для админов)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FaqDto), 201)]
    public async Task<ActionResult<FaqDto>> CreateFaq([FromBody] FaqCreateDto dto)
    {
        var faq = new FaqEntry
        {
            Question = dto.Question,
            Answer = dto.Answer,
            Category = dto.Category,
            DisplayOrder = dto.DisplayOrder
        };

        _context.FaqEntries.Add(faq);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFaq), new { id = faq.Id }, new FaqDto
        {
            Id = faq.Id,
            Question = faq.Question,
            Answer = faq.Answer,
            Category = faq.Category,
            DisplayOrder = faq.DisplayOrder
        });
    }

    /// <summary>
    /// Обновить вопрос FAQ
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FaqDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FaqDto>> UpdateFaq(int id, [FromBody] FaqCreateDto dto)
    {
        var faq = await _context.FaqEntries.FindAsync(id);
        if (faq == null) return NotFound();

        faq.Question = dto.Question;
        faq.Answer = dto.Answer;
        faq.Category = dto.Category;
        faq.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();

        return Ok(new FaqDto
        {
            Id = faq.Id,
            Question = faq.Question,
            Answer = faq.Answer,
            Category = faq.Category,
            DisplayOrder = faq.DisplayOrder
        });
    }

    /// <summary>
    /// Удалить вопрос FAQ
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteFaq(int id)
    {
        var faq = await _context.FaqEntries.FindAsync(id);
        if (faq == null) return NotFound();

        _context.FaqEntries.Remove(faq);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
