namespace OnboardingSystem.Services;

public interface IKeycloakAdminService
{
    /// <summary>
    /// Создать пользователя в Keycloak и назначить ему роли
    /// </summary>
    Task<string?> CreateUserAsync(string email, string fullName, string temporaryPassword, IList<string> roleNames);

    /// <summary>
    /// Удалить пользователя из Keycloak по email
    /// </summary>
    Task DeleteUserAsync(string email);

    /// <summary>
    /// Обновить email/имя пользователя в Keycloak
    /// </summary>
    Task UpdateUserAsync(string oldEmail, string? newEmail, string? newFullName);

    /// <summary>
    /// Назначить роли пользователю в Keycloak
    /// </summary>
    Task AssignRolesAsync(string keycloakUserId, IList<string> roleNames);
}
