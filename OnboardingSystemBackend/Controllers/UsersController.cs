using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using OnboardingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using IAppAuthorizationService = OnboardingSystem.Services.IAuthorizationService;

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
    private readonly IPasswordResetService _passwordResetService;
    private readonly IConfiguration _configuration;
    private readonly IAppAuthorizationService _authorizationService;

    public UsersController(AppDbContext context, ILogger<UsersController> logger, IEmailService emailService, IAuthenticationProvider authProvider, PasswordHasher passwordHasher, IPasswordResetService passwordResetService, IConfiguration configuration, IAppAuthorizationService authorizationService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
        _authProvider = authProvider;
        _passwordHasher = passwordHasher;
        _passwordResetService = passwordResetService;
        _configuration = configuration;
        _authorizationService = authorizationService;
    }


    /// <summary>
    /// Получить список всех пользователей
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), 200)]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var currentUser = await this.GetCurrentUserAsync(_context);
        
        var query = _context.Users
            .Include(u => u.Department)
            .Include(u => u.Mentor)
            .Include(u => u.Roles)
            .Include(u => u.JobTitle)
            .Include(u => u.InverseMentor)
            .AsQueryable();

        if (currentUser != null && !_authorizationService.IsAdmin(currentUser) && !_authorizationService.IsHr(currentUser))
        {
            if (_authorizationService.IsDepartmentHead(currentUser))
            {
                query = query.Where(u => u.DepartmentId == currentUser.DepartmentId);
            }
            else
            {
                query = query.Where(u => u.UserId == currentUser.UserId);
            }
        }

        var users = await query
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
                JobTitleId = u.JobTitleId,
                JobTitle = u.JobTitle != null ? new JobTitleDto { JobTitleId = u.JobTitle.JobTitleId, Title = u.JobTitle.Title, Description = u.JobTitle.Description } : null,
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
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var currentUser = await this.GetCurrentUserAsync(_context);
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Mentor)
            .Include(u => u.Roles)
            .Include(u => u.JobTitle)
            .Include(u => u.InverseMentor)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound();
        }

        // Проверяем доступ
        if (currentUser != null && !_authorizationService.CanViewUser(currentUser, user))
        {
            return Forbid();
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
            JobTitleId = user.JobTitleId,
            JobTitle = user.JobTitle != null ? new JobTitleDto { JobTitleId = user.JobTitle.JobTitleId, Title = user.JobTitle.Title, Description = user.JobTitle.Description } : null,
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
        var currentUser = await this.GetCurrentUserAsync(_context);
        if (currentUser != null && !_authorizationService.CanCreateOrDeleteUser(currentUser))
        {
            return Forbid();
        }

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
            JobTitleId = dto.JobTitleId,
            OnboardingStatus = "Не начат",
            PasswordHash = null  // Пароль будет установлен пользователем позже
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
            .Reference(u => u.JobTitle)
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
            JobTitleId = user.JobTitleId,
            JobTitle = user.JobTitle != null ? new JobTitleDto { JobTitleId = user.JobTitle.JobTitleId, Title = user.JobTitle.Title, Description = user.JobTitle.Description } : null,
            Roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>(),
            TotalXP = user.TotalXP,
            Level = user.Level
        };

        // 🔐 Генерируем токен для установки пароля (действителен 24 часа)
        try
        {
            var token = await _passwordResetService.CreatePasswordSetupTokenAsync(user.UserId, TimeSpan.FromHours(24));
            
            // 📧 Отправляем письмо с ссылкой на установку пароля
            var frontendBaseUrl =
                (_configuration["Frontend:BaseUrl"] ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                // Fallback to current host (works when frontend is served by same host)
                frontendBaseUrl = $"{Request.Scheme}://{Request.Host}";
            }
            var setupUrl = $"{frontendBaseUrl}/auth/set-password?token={token}";
            await _emailService.SendPasswordSetupEmailAsync(user.Email, user.FullName, setupUrl);
            
            _logger.LogInformation($"✅ Пользователь создан: {user.Email}. Письмо с ссылкой для установки пароля отправлено.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Ошибка при отправке письма для установки пароля: {ex.Message}");
            // Не прерываем создание пользователя, он создан, но письмо не отправлено
        }

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
        var currentUser = await this.GetCurrentUserAsync(_context);
        var user = await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.JobTitle)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound();
        }

        if (currentUser == null)
        {
            return Unauthorized();
        }

        var isSelf = currentUser.UserId == user.UserId;
        var isFullManager = _authorizationService.IsAdmin(currentUser) || _authorizationService.IsHr(currentUser);

        if (!isSelf && !_authorizationService.CanEditUser(currentUser, user))
        {
            return Forbid();
        }

        if (isFullManager)
        {
            if (dto.ExternalId != null) user.ExternalId = dto.ExternalId;
            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.DepartmentId.HasValue) user.DepartmentId = dto.DepartmentId.Value;
            if (dto.MentorId.HasValue) user.MentorId = dto.MentorId;
            if (dto.HireDate.HasValue) user.HireDate = dto.HireDate.Value;
            if (dto.OnboardingStatus != null) user.OnboardingStatus = dto.OnboardingStatus;
            if (dto.JobTitleId.HasValue) user.JobTitleId = dto.JobTitleId;
            if (dto.TelegramTag != null) user.TelegramTag = dto.TelegramTag;
            if (dto.Bio != null) user.Bio = dto.Bio;

            if (dto.RoleIds != null)
            {
                var roles = await _context.Roles
                    .Where(r => dto.RoleIds.Contains(r.RoleId))
                    .ToListAsync();
                user.Roles = roles;
            }
        }
        else if (_authorizationService.IsDepartmentHead(currentUser) && !isSelf)
        {
            if (!_authorizationService.CanManageMentees(currentUser, user.DepartmentId))
            {
                return Forbid();
            }

            if (dto.MentorId.HasValue)
            {
                var mentor = await _context.Users.FindAsync(dto.MentorId.Value);
                if (mentor == null || mentor.DepartmentId != user.DepartmentId)
                {
                    return BadRequest(new { message = "Наставник должен быть из того же отдела" });
                }
                user.MentorId = dto.MentorId;
            }

            if (dto.OnboardingStatus != null) user.OnboardingStatus = dto.OnboardingStatus;
        }
        else if (isSelf)
        {
            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.TelegramTag != null) user.TelegramTag = dto.TelegramTag;
            if (dto.Bio != null) user.Bio = dto.Bio;
        }
        else
        {
            return Forbid();
        }

        await _context.SaveChangesAsync();

        await _context.Entry(user)
            .Reference(u => u.Department)
            .LoadAsync();
        await _context.Entry(user)
            .Reference(u => u.Mentor)
            .LoadAsync();
        await _context.Entry(user)
            .Reference(u => u.JobTitle)
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
            JobTitleId = user.JobTitleId,
            JobTitle = user.JobTitle != null ? new JobTitleDto { JobTitleId = user.JobTitle.JobTitleId, Title = user.JobTitle.Title, Description = user.JobTitle.Description } : null,
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
        var currentUser = await this.GetCurrentUserAsync(_context);
        if (currentUser != null && !_authorizationService.CanCreateOrDeleteUser(currentUser))
        {
            return Forbid();
        }

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
        var currentUser = await this.GetCurrentUserAsync(_context);
        var mentor = await _context.Users.FindAsync(mentorId);
        if (mentor == null)
        {
            return NotFound();
        }

        if (currentUser != null
            && currentUser.UserId != mentorId
            && !_authorizationService.IsAdmin(currentUser)
            && !_authorizationService.IsHr(currentUser)
            && !(_authorizationService.IsDepartmentHead(currentUser)
                && currentUser.DepartmentId == mentor.DepartmentId))
        {
            return Forbid();
        }

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
            .Include(u => u.JobTitle)
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
            JobTitleId = user.JobTitleId,
            JobTitle = user.JobTitle != null ? new JobTitleDto { JobTitleId = user.JobTitle.JobTitleId, Title = user.JobTitle.Title, Description = user.JobTitle.Description } : null,
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

    /// <summary>
    /// Установить пароль по токену (публичный endpoint, не требует авторизации)
    /// </summary>
    [HttpPost("set-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SetPasswordResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<SetPasswordResponseDto>> SetPassword([FromBody] SetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new SetPasswordResponseDto
            {
                Success = false,
                ErrorMessage = "Некорректные данные"
            });
        }

        // Валидация пароля
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
        {
            return BadRequest(new SetPasswordResponseDto
            {
                Success = false,
                ErrorMessage = "Пароль должен быть не менее 8 символов"
            });
        }

        if (dto.Password != dto.ConfirmPassword)
        {
            return BadRequest(new SetPasswordResponseDto
            {
                Success = false,
                ErrorMessage = "Пароли не совпадают"
            });
        }

        // Проверяем токен
        var (isValid, userId) = await _passwordResetService.ValidateTokenAsync(dto.Token);
        if (!isValid || !userId.HasValue)
        {
            return Unauthorized(new SetPasswordResponseDto
            {
                Success = false,
                ErrorMessage = "Токен недействителен или истек. Запросите новую ссылку."
            });
        }

        // Устанавливаем пароль
        var success = await _passwordResetService.SetPasswordWithTokenAsync(dto.Token, dto.Password, _passwordHasher);
        
        if (!success)
        {
            return Unauthorized(new SetPasswordResponseDto
            {
                Success = false,
                ErrorMessage = "Ошибка при установке пароля. Попробуйте снова или запросите новую ссылку."
            });
        }

        _logger.LogInformation($"✅ Пароль успешно установлен для пользователя с ID {userId}");

        return Ok(new SetPasswordResponseDto
        {
            Success = true,
            Message = "Пароль успешно установлен. Теперь вы можете авторизироваться."
        });
    }
}


