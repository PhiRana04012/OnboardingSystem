using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(AppDbContext context, ILogger<DepartmentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех подразделений
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DepartmentDto>), 200)]
    public async Task<ActionResult<List<DepartmentDto>>> GetDepartments()
    {
        var departments = await _context.Departments
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                Name = d.Name,
                UserCount = d.Users.Count,
                ModuleCount = d.Modules.Count
            })
            .ToListAsync();

        return Ok(departments);
    }

    /// <summary>
    /// Получить подразделение по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DepartmentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == id);

        if (department == null)
        {
            return NotFound();
        }

        await _context.Entry(department)
            .Collection(d => d.Users)
            .LoadAsync();
        await _context.Entry(department)
            .Collection(d => d.Modules)
            .LoadAsync();

        var departmentDto = new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            Name = department.Name,
            UserCount = department.Users.Count,
            ModuleCount = department.Modules.Count
        };

        return Ok(departmentDto);
    }

    /// <summary>
    /// Создать новое подразделение
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DepartmentDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var department = new Department
        {
            Name = dto.Name
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        var departmentDto = new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            Name = department.Name,
            UserCount = 0,
            ModuleCount = 0
        };

        return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId }, departmentDto);
    }

    /// <summary>
    /// Обновить подразделение
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DepartmentDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == id);

        if (department == null)
        {
            return NotFound();
        }

        if (dto.Name != null)
        {
            department.Name = dto.Name;
        }

        await _context.SaveChangesAsync();

        await _context.Entry(department)
            .Collection(d => d.Users)
            .LoadAsync();
        await _context.Entry(department)
            .Collection(d => d.Modules)
            .LoadAsync();

        var departmentDto = new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            Name = department.Name,
            UserCount = department.Users.Count,
            ModuleCount = department.Modules.Count
        };

        return Ok(departmentDto);
    }

    /// <summary>
    /// Удалить подразделение
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


