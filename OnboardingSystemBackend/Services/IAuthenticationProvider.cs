namespace OnboardingSystem.Services
{
    /// <summary>
    /// Интерфейс для различных методов аутентификации
    /// Позволяет легко переключаться между Local, Keycloak, AD и другими провайдерами
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Аутентифицировать пользователя
        /// </summary>
        Task<AuthResult> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Получить пользователя по внешнему ID
        /// </summary>
        Task<AuthResult?> GetUserAsync(string externalId);

        /// <summary>
        /// Валидировать токен
        /// </summary>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Получить имя провайдера
        /// </summary>
        string ProviderName { get; }
    }

    /// <summary>
    /// Результат аутентификации
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ExternalId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
