using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ModulesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ModulesController> _logger;

    public ModulesController(AppDbContext context, ILogger<ModulesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех модулей
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ModuleDto>), 200)]
    public async Task<ActionResult<List<ModuleDto>>> GetModules([FromQuery] int? departmentId, [FromQuery] bool? isMandatory)
    {
        var query = _context.Modules
            .Include(m => m.Department)
            .Include(m => m.Questions)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(m => m.DepartmentId == departmentId);
        }

        if (isMandatory.HasValue)
        {
            query = query.Where(m => m.IsMandatory == isMandatory.Value);
        }

        var modules = await query
            .Select(m => new ModuleDto
            {
                ModuleId = m.ModuleId,
                Title = m.Title,
                Description = m.Description,
                Content = m.Content,
                IsMandatory = m.IsMandatory,
                DepartmentId = m.DepartmentId,
                DepartmentName = m.Department != null ? m.Department.Name : null,
                PassingScore = m.PassingScore,
                MaxAttempts = m.MaxAttempts,
                QuestionCount = m.Questions.Count
            })
            .ToListAsync();

        return Ok(modules);
    }

    /// <summary>
    /// Получить модуль по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ModuleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ModuleDto>> GetModule(int id)
    {
        var module = await _context.Modules
            .Include(m => m.Department)
            .Include(m => m.Questions)
            .FirstOrDefaultAsync(m => m.ModuleId == id);

        if (module == null)
        {
            return NotFound();
        }

        var moduleDto = new ModuleDto
        {
            ModuleId = module.ModuleId,
            Title = module.Title,
            Description = module.Description,
            Content = module.Content,
            IsMandatory = module.IsMandatory,
            DepartmentId = module.DepartmentId,
            DepartmentName = module.Department != null ? module.Department.Name : null,
            PassingScore = module.PassingScore,
            MaxAttempts = module.MaxAttempts,
            QuestionCount = module.Questions.Count
        };

        return Ok(moduleDto);
    }

    /// <summary>
    /// Создать новый модуль
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ModuleDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ModuleDto>> CreateModule([FromBody] CreateModuleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var module = new Module
        {
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            IsMandatory = dto.IsMandatory,
            DepartmentId = dto.DepartmentId,
            PassingScore = dto.PassingScore,
            MaxAttempts = dto.MaxAttempts
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        await _context.Entry(module)
            .Reference(m => m.Department)
            .LoadAsync();
        await _context.Entry(module)
            .Collection(m => m.Questions)
            .LoadAsync();

        var moduleDto = new ModuleDto
        {
            ModuleId = module.ModuleId,
            Title = module.Title,
            Description = module.Description,
            Content = module.Content,
            IsMandatory = module.IsMandatory,
            DepartmentId = module.DepartmentId,
            DepartmentName = module.Department != null ? module.Department.Name : null,
            PassingScore = module.PassingScore,
            MaxAttempts = module.MaxAttempts,
            QuestionCount = module.Questions.Count
        };

        return CreatedAtAction(nameof(GetModule), new { id = module.ModuleId }, moduleDto);
    }

    /// <summary>
    /// Обновить модуль
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ModuleDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ModuleDto>> UpdateModule(int id, [FromBody] UpdateModuleDto dto)
    {
        var module = await _context.Modules
            .Include(m => m.Department)
            .Include(m => m.Questions)
            .FirstOrDefaultAsync(m => m.ModuleId == id);

        if (module == null)
        {
            return NotFound();
        }

        if (dto.Title != null) module.Title = dto.Title;
        if (dto.Description != null) module.Description = dto.Description;
        if (dto.Content != null) module.Content = dto.Content;
        if (dto.IsMandatory.HasValue) module.IsMandatory = dto.IsMandatory.Value;
        if (dto.DepartmentId.HasValue) module.DepartmentId = dto.DepartmentId;
        if (dto.PassingScore.HasValue) module.PassingScore = dto.PassingScore.Value;
        if (dto.MaxAttempts.HasValue) module.MaxAttempts = dto.MaxAttempts.Value;

        await _context.SaveChangesAsync();

        var moduleDto = new ModuleDto
        {
            ModuleId = module.ModuleId,
            Title = module.Title,
            Description = module.Description,
            Content = module.Content,
            IsMandatory = module.IsMandatory,
            DepartmentId = module.DepartmentId,
            DepartmentName = module.Department != null ? module.Department.Name : null,
            PassingScore = module.PassingScore,
            MaxAttempts = module.MaxAttempts,
            QuestionCount = module.Questions.Count
        };

        return Ok(moduleDto);
    }

    /// <summary>
    /// Удалить модуль
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteModule(int id)
    {
        var module = await _context.Modules.FindAsync(id);
        if (module == null)
        {
            return NotFound();
        }

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


