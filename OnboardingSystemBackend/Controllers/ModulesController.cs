using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using OnboardingSystem.Services;
using IAppAuthorizationService = OnboardingSystem.Services.IAuthorizationService;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ModulesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ModulesController> _logger;
    private readonly IAppAuthorizationService _authorizationService;

    public ModulesController(AppDbContext context, ILogger<ModulesController> logger, IAppAuthorizationService authorizationService)
    {
        _context = context;
        _logger = logger;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Получить список всех модулей
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ModuleDto>), 200)]
    public async Task<ActionResult<List<ModuleDto>>> GetModules([FromQuery] int? departmentId, [FromQuery] bool? isMandatory)
    {
        var currentUser = await this.GetCurrentUserAsync(_context);

        if (currentUser != null
            && _authorizationService.IsDepartmentHead(currentUser)
            && !_authorizationService.IsAdmin(currentUser)
            && !_authorizationService.IsHr(currentUser)
            )
        {
            departmentId = currentUser.DepartmentId;
        }

        var query = _context.Modules
            .Include(m => m.Department)
            .Include(m => m.ModuleDepartments)
            .ThenInclude(md => md.Department)
            .Include(m => m.Questions)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(m =>
                m.DepartmentId == departmentId ||
                m.ModuleDepartments.Any(md => md.DepartmentId == departmentId));
        }

        if (isMandatory.HasValue)
        {
            query = query.Where(m => m.IsMandatory == isMandatory.Value);
        }

        var modules = await query.ToListAsync();

        return Ok(modules.Select(MapModuleToDto).ToList());
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
            .Include(m => m.ModuleDepartments)
            .ThenInclude(md => md.Department)
            .Include(m => m.Questions)
            .FirstOrDefaultAsync(m => m.ModuleId == id);

        if (module == null)
        {
            return NotFound();
        }

        var currentUser = await this.GetCurrentUserAsync(_context);
        if (currentUser != null && !_authorizationService.CanManageModule(currentUser, module))
        {
            return Forbid();
        }

        return Ok(MapModuleToDto(module));
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

        var currentUser = await this.GetCurrentUserAsync(_context);
        var selectedDepartmentIds = NormalizeDepartmentIds(dto.DepartmentIds, dto.DepartmentId);

        if (currentUser != null
            && !_authorizationService.IsAdmin(currentUser)
            && !_authorizationService.IsHr(currentUser))
        {
            if (!_authorizationService.IsDepartmentHead(currentUser)
                || selectedDepartmentIds.Count != 1
                || selectedDepartmentIds[0] != currentUser.DepartmentId)
            {
                return Forbid();
            }
        }

        var module = new Module
        {
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            IsMandatory = dto.IsMandatory,
            PassingScore = dto.PassingScore,
            MaxAttempts = dto.MaxAttempts
        };

        module.DepartmentId = selectedDepartmentIds.Count == 1 ? selectedDepartmentIds[0] : null;
        module.ModuleDepartments = selectedDepartmentIds
            .Select(id => new ModuleDepartment { DepartmentId = id })
            .ToList();

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        await _context.Entry(module)
            .Reference(m => m.Department)
            .LoadAsync();
        await _context.Entry(module)
            .Collection(m => m.ModuleDepartments)
            .Query()
            .Include(md => md.Department)
            .LoadAsync();
        await _context.Entry(module)
            .Collection(m => m.Questions)
            .LoadAsync();

        return CreatedAtAction(nameof(GetModule), new { id = module.ModuleId }, MapModuleToDto(module));
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
            .Include(m => m.ModuleDepartments)
            .ThenInclude(md => md.Department)
            .Include(m => m.Questions)
            .FirstOrDefaultAsync(m => m.ModuleId == id);

        if (module == null)
        {
            return NotFound();
        }

        var currentUser = await this.GetCurrentUserAsync(_context);
        if (currentUser != null && !_authorizationService.CanManageModule(currentUser, module))
        {
            return Forbid();
        }

        if (dto.Title != null) module.Title = dto.Title;
        if (dto.Description != null) module.Description = dto.Description;
        if (dto.Content != null) module.Content = dto.Content;
        if (dto.IsMandatory.HasValue) module.IsMandatory = dto.IsMandatory.Value;
        if (dto.PassingScore.HasValue) module.PassingScore = dto.PassingScore.Value;
        if (dto.MaxAttempts.HasValue) module.MaxAttempts = dto.MaxAttempts.Value;

        if (dto.DepartmentIds != null || dto.DepartmentId.HasValue)
        {
            var selectedDepartmentIds = NormalizeDepartmentIds(dto.DepartmentIds, dto.DepartmentId);

            if (currentUser != null
                && !_authorizationService.IsAdmin(currentUser)
                && !_authorizationService.IsHr(currentUser))
            {
                if (!_authorizationService.IsDepartmentHead(currentUser)
                    || selectedDepartmentIds.Count != 1
                    || selectedDepartmentIds[0] != currentUser.DepartmentId)
                {
                    return Forbid();
                }
            }

            module.ModuleDepartments.Clear();
            foreach (var depId in selectedDepartmentIds)
            {
                module.ModuleDepartments.Add(new ModuleDepartment
                {
                    ModuleId = module.ModuleId,
                    DepartmentId = depId
                });
            }

            module.DepartmentId = selectedDepartmentIds.Count == 1 ? selectedDepartmentIds[0] : null;
        }

        await _context.SaveChangesAsync();

        return Ok(MapModuleToDto(module));
    }

    /// <summary>
    /// Удалить модуль
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteModule(int id)
    {
        var module = await _context.Modules
            .Include(m => m.ModuleDepartments)
            .FirstOrDefaultAsync(m => m.ModuleId == id);
        if (module == null)
        {
            return NotFound();
        }

        var currentUser = await this.GetCurrentUserAsync(_context);
        if (currentUser != null && !_authorizationService.CanManageModule(currentUser, module))
        {
            return Forbid();
        }

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static List<int> NormalizeDepartmentIds(List<int>? departmentIds, int? fallbackDepartmentId)
    {
        if (departmentIds != null)
        {
            return departmentIds
                .Distinct()
                .ToList();
        }

        if (fallbackDepartmentId.HasValue)
        {
            return new List<int> { fallbackDepartmentId.Value };
        }

        return new List<int>();
    }

    private static ModuleDto MapModuleToDto(Module module)
    {
        var relationDepartmentIds = module.ModuleDepartments
            .Select(md => md.DepartmentId)
            .Distinct()
            .ToList();

        var relationDepartmentNames = module.ModuleDepartments
            .Select(md => md.Department?.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .Distinct()
            .ToList();

        if (!relationDepartmentIds.Any() && module.DepartmentId.HasValue)
        {
            relationDepartmentIds.Add(module.DepartmentId.Value);
            if (!string.IsNullOrWhiteSpace(module.Department?.Name))
            {
                relationDepartmentNames.Add(module.Department.Name);
            }
        }

        return new ModuleDto
        {
            ModuleId = module.ModuleId,
            Title = module.Title,
            Description = module.Description,
            Content = module.Content,
            IsMandatory = module.IsMandatory,
            DepartmentId = module.DepartmentId,
            DepartmentName = module.Department?.Name,
            DepartmentIds = relationDepartmentIds,
            DepartmentNames = relationDepartmentNames,
            PassingScore = module.PassingScore,
            MaxAttempts = module.MaxAttempts,
            QuestionCount = module.Questions.Count
        };
    }
}


