using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using OnboardingSystem.Services;
using Microsoft.AspNetCore.Authorization;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<UsersController> _logger;
    private readonly IEmailService _emailService;
    private readonly IAuthenticationProvider _authProvider;
    private readonly PasswordHasher _passwordHasher;

    public UsersController(AppDbContext context, ILogger<UsersController> logger, IEmailService emailService, IAuthenticationProvider authProvider, PasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
        _authProvider = authProvider;
        _passwordHasher = passwordHasher;
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
                TelegramTag = u.TelegramTag,
                Bio = u.Bio,
                Roles = u.Roles.Select(r => r.RoleName).ToList() ?? new List<string>(),
                HasMentees = u.InverseMentor.Any(),
                TotalXP = u.TotalXP,
                Level = u.Level
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
            .Include(u => u.InverseMentor)
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
            TelegramTag = user.TelegramTag,
            Bio = user.Bio,
            Roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>(),
            HasMentees = user.InverseMentor != null && user.InverseMentor.Any(),
            TotalXP = user.TotalXP,
            Level = user.Level
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

        // Проверка на дубликат email
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
            return Conflict(new { message = $"Пользователь с email '{dto.Email}' уже существует" });
        }

        var user = new User
        {
            ExternalId = dto.ExternalId,
            FullName = dto.FullName,
            Email = dto.Email,
            DepartmentId = dto.DepartmentId,
            MentorId = dto.MentorId,
            HireDate = dto.HireDate,
            JobTitle = dto.JobTitle,
            OnboardingStatus = "Не начат"
        };

        // Установка пароля, если передан
        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(dto.Password);
        }

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
            Roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>(),
            TotalXP = user.TotalXP,
            Level = user.Level
        };

        // Send Welcome Email
        await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

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
        if (dto.JobTitle != null) user.JobTitle = dto.JobTitle;
        if (dto.TelegramTag != null) user.TelegramTag = dto.TelegramTag;
        if (dto.Bio != null) user.Bio = dto.Bio;

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
            Roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>(),
            TotalXP = user.TotalXP,
            Level = user.Level
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

    /// <summary>
    /// Получить список подопечных для наставника
    /// </summary>
    [HttpGet("mentor/{mentorId}/mentees")]
    [ProducesResponseType(typeof(List<object>), 200)]
    public async Task<ActionResult> GetMentees(int mentorId)
    {
        var mentees = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Roles)
            .Where(u => u.MentorId == mentorId)
            .Select(u => new
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                DepartmentName = u.Department != null ? u.Department.Name : "Не указано",
                JobTitle = u.JobTitle,
                HireDate = u.HireDate,
                OnboardingStatus = u.OnboardingStatus,
                TelegramTag = u.TelegramTag,
                Level = u.Level,
                TotalXP = u.TotalXP
            })
            .ToListAsync();

        return Ok(mentees);
    }

    /// <summary>
    /// Аутентифицировать пользователя по email и пароль
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Используем провайдер аутентификации
        var authResult = await _authProvider.AuthenticateAsync(dto.Email, dto.Password);

        if (!authResult.Success)
        {
            return Unauthorized(new LoginResponseDto
            {
                Success = false,
                ErrorMessage = authResult.ErrorMessage ?? "Ошибка аутентификации"
            });
        }

        // Получаем полные данные пользователя
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Mentor)
            .Include(u => u.Roles)
            .Include(u => u.InverseMentor)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            return Unauthorized(new LoginResponseDto
            {
                Success = false,
                ErrorMessage = "Пользователь не найден"
            });
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
            TelegramTag = user.TelegramTag,
            Bio = user.Bio,
            Roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>(),
            HasMentees = user.InverseMentor != null && user.InverseMentor.Any(),
            TotalXP = user.TotalXP,
            Level = user.Level
        };

        return Ok(new LoginResponseDto
        {
            Success = true,
            Token = authResult.Token,
            User = userDto
        });
    }
}


