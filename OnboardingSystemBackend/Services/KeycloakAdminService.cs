using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OnboardingSystem.Services;

/// <summary>
/// Сервис для работы с Keycloak Admin REST API.
/// Позволяет создавать, удалять и обновлять пользователей в Keycloak
/// синхронно с созданием пользователей в нашей БД.
/// </summary>
public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<KeycloakAdminService> _logger;

    private string AdminUrl => _config["Keycloak:AdminUrl"]!;
    private string Realm => _config["Keycloak:Realm"]!;
    private string AdminUser => _config["Keycloak:AdminUser"]!;
    private string AdminPassword => _config["Keycloak:AdminPassword"]!;
    private string ClientId => _config["Keycloak:ClientId"]!;

    public KeycloakAdminService(HttpClient httpClient, IConfiguration config, ILogger<KeycloakAdminService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    // ─── Получить admin access token ───────────────────────────────────────────
    private async Task<string> GetAdminTokenAsync()
    {
        var tokenUrl = $"{AdminUrl}/realms/master/protocol/openid-connect/token";

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "admin-cli"),
            new KeyValuePair<string, string>("username", AdminUser),
            new KeyValuePair<string, string>("password", AdminPassword),
        });

        var response = await _httpClient.PostAsync(tokenUrl, form);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    // ─── Найти Keycloak userId по email ────────────────────────────────────────
    private async Task<string?> FindUserIdByEmailAsync(string token, string email)
    {
        var url = $"{AdminUrl}/admin/realms/{Realm}/users?email={Uri.EscapeDataString(email)}&exact=true";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var users = JsonDocument.Parse(json).RootElement;

        if (users.GetArrayLength() == 0) return null;
        return users[0].GetProperty("id").GetString();
    }

    // ─── Найти ID роли клиента по имени ────────────────────────────────────────
    private async Task<(string roleId, string roleName)?> FindClientRoleAsync(string token, string clientUuid, string roleName)
    {
        var url = $"{AdminUrl}/admin/realms/{Realm}/clients/{clientUuid}/roles/{Uri.EscapeDataString(roleName)}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var role = JsonDocument.Parse(json).RootElement;
        return (role.GetProperty("id").GetString()!, role.GetProperty("name").GetString()!);
    }

    // ─── Получить UUID клиента по clientId ─────────────────────────────────────
    private async Task<string?> GetClientUuidAsync(string token)
    {
        var url = $"{AdminUrl}/admin/realms/{Realm}/clients?clientId={Uri.EscapeDataString(ClientId)}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var clients = JsonDocument.Parse(json).RootElement;
        if (clients.GetArrayLength() == 0) return null;
        return clients[0].GetProperty("id").GetString();
    }

    // ─── Создать пользователя ───────────────────────────────────────────────────
    public async Task<string?> CreateUserAsync(string email, string fullName, string temporaryPassword, IList<string> roleNames)
    {
        try
        {
            var token = await GetAdminTokenAsync();
            var names = fullName.Trim().Split(' ', 2);
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[1] : "";

            // 1. Создать пользователя
            var userPayload = new
            {
                username = email,
                email = email,
                firstName = firstName,
                lastName = lastName,
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                    new { type = "password", value = temporaryPassword, temporary = true }
                }
            };

            var createUrl = $"{AdminUrl}/admin/realms/{Realm}/users";
            var createRequest = new HttpRequestMessage(HttpMethod.Post, createUrl);
            createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            createRequest.Content = new StringContent(
                JsonSerializer.Serialize(userPayload),
                Encoding.UTF8, "application/json");

            var createResponse = await _httpClient.SendAsync(createRequest);
            if (!createResponse.IsSuccessStatusCode)
            {
                var err = await createResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("Keycloak create user failed: {Status} {Error}", createResponse.StatusCode, err);
                return null;
            }

            // 2. Получить ID нового пользователя
            var keycloakUserId = await FindUserIdByEmailAsync(token, email);
            if (keycloakUserId == null) return null;

            // 3. Назначить роли
            if (roleNames.Any())
                await AssignRolesInternalAsync(token, keycloakUserId, roleNames);

            _logger.LogInformation("Keycloak: пользователь {Email} создан, ID={Id}", email, keycloakUserId);
            return keycloakUserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании пользователя {Email} в Keycloak", email);
            return null;
        }
    }

    // ─── Удалить пользователя ───────────────────────────────────────────────────
    public async Task DeleteUserAsync(string email)
    {
        try
        {
            var token = await GetAdminTokenAsync();
            var userId = await FindUserIdByEmailAsync(token, email);
            if (userId == null)
            {
                _logger.LogWarning("Keycloak: пользователь {Email} не найден при удалении", email);
                return;
            }

            var deleteUrl = $"{AdminUrl}/admin/realms/{Realm}/users/{userId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                _logger.LogInformation("Keycloak: пользователь {Email} удалён", email);
            else
                _logger.LogWarning("Keycloak: ошибка при удалении {Email}: {Status}", email, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении пользователя {Email} из Keycloak", email);
        }
    }

    // ─── Обновить пользователя ──────────────────────────────────────────────────
    public async Task UpdateUserAsync(string oldEmail, string? newEmail, string? newFullName)
    {
        try
        {
            var token = await GetAdminTokenAsync();
            var userId = await FindUserIdByEmailAsync(token, oldEmail);
            if (userId == null)
            {
                _logger.LogWarning("Keycloak: пользователь {Email} не найден при обновлении", oldEmail);
                return;
            }

            var updatePayload = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(newEmail))
            {
                updatePayload["email"] = newEmail;
                updatePayload["username"] = newEmail;
            }
            if (!string.IsNullOrEmpty(newFullName))
            {
                var names = newFullName.Trim().Split(' ', 2);
                updatePayload["firstName"] = names[0];
                updatePayload["lastName"] = names.Length > 1 ? names[1] : "";
            }

            if (!updatePayload.Any()) return;

            var updateUrl = $"{AdminUrl}/admin/realms/{Realm}/users/{userId}";
            var request = new HttpRequestMessage(HttpMethod.Put, updateUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(
                JsonSerializer.Serialize(updatePayload),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                _logger.LogInformation("Keycloak: пользователь {Email} обновлён", oldEmail);
            else
                _logger.LogWarning("Keycloak: ошибка обновления {Email}: {Status}", oldEmail, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении пользователя {Email} в Keycloak", oldEmail);
        }
    }

    // ─── Назначить роли (публичный) ─────────────────────────────────────────────
    public async Task AssignRolesAsync(string keycloakUserId, IList<string> roleNames)
    {
        try
        {
            var token = await GetAdminTokenAsync();
            await AssignRolesInternalAsync(token, keycloakUserId, roleNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при назначении ролей пользователю {Id} в Keycloak", keycloakUserId);
        }
    }

    // ─── Назначить роли (внутренний) ────────────────────────────────────────────
    private async Task AssignRolesInternalAsync(string token, string keycloakUserId, IList<string> roleNames)
    {
        var clientUuid = await GetClientUuidAsync(token);
        if (clientUuid == null)
        {
            _logger.LogWarning("Keycloak: клиент {ClientId} не найден", ClientId);
            return;
        }

        var rolesToAssign = new List<object>();
        foreach (var roleName in roleNames)
        {
            var role = await FindClientRoleAsync(token, clientUuid, roleName);
            if (role.HasValue)
                rolesToAssign.Add(new { id = role.Value.roleId, name = role.Value.roleName });
            else
                _logger.LogWarning("Keycloak: роль '{Role}' не найдена в клиенте {ClientId}", roleName, ClientId);
        }

        if (!rolesToAssign.Any()) return;

        var assignUrl = $"{AdminUrl}/admin/realms/{Realm}/users/{keycloakUserId}/role-mappings/clients/{clientUuid}";
        var request = new HttpRequestMessage(HttpMethod.Post, assignUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(
            JsonSerializer.Serialize(rolesToAssign),
            Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            _logger.LogInformation("Keycloak: роли {Roles} назначены пользователю {Id}", string.Join(", ", roleNames), keycloakUserId);
        else
            _logger.LogWarning("Keycloak: ошибка назначения ролей: {Status}", response.StatusCode);
    }
}
