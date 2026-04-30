using OnboardingSystem.Data;
using OnboardingSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnboardingSystem.Services
{
    /// <summary>
    /// Провайдер локальной аутентификации с хранением паролей в БД
    /// </summary>
    public class LocalAuthProvider : IAuthenticationProvider
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly ILogger<LocalAuthProvider> _logger;

        public string ProviderName => "Local";

        public LocalAuthProvider(
            AppDbContext context,
            PasswordHasher passwordHasher,
            JwtTokenGenerator tokenGenerator,
            ILogger<LocalAuthProvider> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        /// <summary>
        /// Аутентифицировать пользователя по email и пароль
        /// </summary>
        public async Task<AuthResult> AuthenticateAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return new AuthResult 
                    { 
                        Success = false,
                        ErrorMessage = "Email и пароль обязательны"
                    };
                }

                // Найти пользователя по email
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning($"Попытка входа для несуществующего пользователя: {email}");
                    return new AuthResult 
                    { 
                        Success = false,
                        ErrorMessage = "Неверный email или пароль"
                    };
                }

                // Проверяем, заблокирован ли пользователь
                if (!user.IsActive)
                {
                    _logger.LogWarning($"Попытка входа для заблокированного пользователя: {email}");
                    return new AuthResult 
                    { 
                        Success = false,
                        ErrorMessage = "Пользователь заблокирован"
                    };
                }

                // Проверяем пароль
                if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning($"Неверный пароль для пользователя: {email}");
                    return new AuthResult 
                    { 
                        Success = false,
                        ErrorMessage = "Неверный email или пароль"
                    };
                }

                // Генерируем токен
                var roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>();
                var token = _tokenGenerator.GenerateToken(user.UserId, user.Email, user.FullName, roles);

                // Обновляем время последнего входа
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Успешная аутентификация пользователя: {email}");

                return new AuthResult
                {
                    Success = true,
                    ExternalId = user.UserId.ToString(),
                    Email = user.Email,
                    FullName = user.FullName,
                    Token = token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка аутентификации: {ex.Message}");
                return new AuthResult 
                { 
                    Success = false,
                    ErrorMessage = "Ошибка аутентификации"
                };
            }
        }

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        public async Task<AuthResult?> GetUserAsync(string externalId)
        {
            try
            {
                if (!int.TryParse(externalId, out var userId))
                    return null;

                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                    return null;

                var roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>();
                var token = _tokenGenerator.GenerateToken(user.UserId, user.Email, user.FullName, roles);

                return new AuthResult
                {
                    Success = true,
                    ExternalId = user.UserId.ToString(),
                    Email = user.Email,
                    FullName = user.FullName,
                    Token = token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения пользователя: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Валидировать JWT токен
        /// </summary>
        public Task<bool> ValidateTokenAsync(string token)
        {
            return Task.FromResult(_tokenGenerator.ValidateToken(token));
        }
    }
}
