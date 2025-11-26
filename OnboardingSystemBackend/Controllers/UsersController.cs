using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(AppDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех пользователей
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), 200)]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Mentor)
            .Include(u => u.Roles)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                ExternalId = u.ExternalId,
                FullName = u.FullName,
                Email = u.Email,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department != null ? u.Department.Name : "Не указано",
                MentorId = u.MentorId,
                MentorName = u.Mentor != null ? u.Mentor.FullName : null,
                HireDate = u.HireDate,
                OnboardingStatus = u.OnboardingStatus,
                JobTitle = u.JobTitle,
                Roles = u.Roles.Select(r => r.RoleName).ToList() ?? new List<string>()
            })
            .ToListAsync();

        return Ok(users);
    }

    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Mentor)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound();
        }

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

    /// <summary>
    /// Создать нового пользователя
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new User
        {
            ExternalId = dto.ExternalId,
            FullName = dto.FullName,
            Email = dto.Email,
            DepartmentId = dto.DepartmentId,
            MentorId = dto.MentorId,
            HireDate = dto.HireDate,
            OnboardingStatus = "Не начат"
        };

        if (dto.RoleIds.Any())
        {
            var roles = await _context.Roles
                .Where(r => dto.RoleIds.Contains(r.RoleId))
                .ToListAsync();
            user.Roles = roles;
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _context.Entry(user)
            .Reference(u => u.Department)
            .LoadAsync();
        await _context.Entry(user)
            .Reference(u => u.Mentor)
            .LoadAsync();
        await _context.Entry(user)
            .Collection(u => u.Roles)
            .LoadAsync();

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

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userDto);
    }

    /// <summary>
    /// Обновить пользователя
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound();
        }

        if (dto.ExternalId != null) user.ExternalId = dto.ExternalId;
        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.DepartmentId.HasValue) user.DepartmentId = dto.DepartmentId.Value;
        if (dto.MentorId.HasValue) user.MentorId = dto.MentorId;
        if (dto.HireDate.HasValue) user.HireDate = dto.HireDate.Value;
        if (dto.OnboardingStatus != null) user.OnboardingStatus = dto.OnboardingStatus;

        if (dto.RoleIds != null)
        {
            var roles = await _context.Roles
                .Where(r => dto.RoleIds.Contains(r.RoleId))
                .ToListAsync();
            user.Roles = roles;
        }

        await _context.SaveChangesAsync();

        await _context.Entry(user)
            .Reference(u => u.Department)
            .LoadAsync();
        await _context.Entry(user)
            .Reference(u => u.Mentor)
            .LoadAsync();
        await _context.Entry(user)
            .Collection(u => u.Roles)
            .LoadAsync();

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

    /// <summary>
    /// Удалить пользователя
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


