using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using System.Text.Json;

namespace OnboardingSystem.Services;

/// <summary>
/// Сервис интеграции с RIMS API
/// </summary>
public class RimsIntegrationService : IRimsIntegrationService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RimsIntegrationService> _logger;
    private readonly string _rimsApiBaseUrl;
    private readonly string _searchEndpoint;

    public RimsIntegrationService(
        AppDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<RimsIntegrationService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient("RimsApi");
        _logger = logger;
        _rimsApiBaseUrl = configuration["RimsApi:BaseUrl"] ?? string.Empty;
        _searchEndpoint = configuration["RimsApi:SearchEndpoint"] ?? "/api/persons/search";

        if (string.IsNullOrEmpty(_rimsApiBaseUrl))
        {
            _logger.LogWarning("RimsApi:BaseUrl не настроен в appsettings.json");
        }
    }

    /// <summary>
    /// Поиск пользователя в RIMS по Uid
    /// </summary>
    public async Task<RimsPersonItem?> FindPersonByUidAsync(string uid, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_rimsApiBaseUrl))
            {
                _logger.LogWarning("RIMS API URL не настроен, возвращаю null");
                return null;
            }

            // Endpoint настраивается через RimsApi:SearchEndpoint в appsettings.json
            // По умолчанию: /api/persons/search
            var endpoint = _searchEndpoint.TrimStart('/');
            var url = $"{_rimsApiBaseUrl.TrimEnd('/')}/{endpoint}?uid={Uri.EscapeDataString(uid)}";
            
            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.GetAsync(url, ct);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Не удалось подключиться к RIMS API. URL: {Url}", url);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Таймаут при подключении к RIMS API. URL: {Url}", url);
                return null;
            }
            
            if (response != null && !response.IsSuccessStatusCode)
            {
                _logger.LogWarning("RIMS API вернул ошибку: {StatusCode}", response.StatusCode);
                return null;
            }

            if (response == null)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var rimsResponse = JsonSerializer.Deserialize<RimsPersonResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (rimsResponse?.Status == true && rimsResponse.Data != null)
            {
                // Найти Entity с типом "Person"
                var personEntity = rimsResponse.Data.FirstOrDefault(e => e.Entity == "Person");
                return personEntity?.Result?.Items?.FirstOrDefault();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске пользователя в RIMS по Uid: {Uid}", uid);
            return null;
        }
    }

    /// <summary>
    /// Поиск пользователя в RIMS по Email
    /// </summary>
    public async Task<RimsPersonItem?> FindPersonByEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_rimsApiBaseUrl))
            {
                _logger.LogWarning("RIMS API URL не настроен, возвращаю null");
                return null;
            }

            // Endpoint настраивается через RimsApi:SearchEndpoint в appsettings.json
            // По умолчанию: /api/persons/search
            var endpoint = _searchEndpoint.TrimStart('/');
            var url = $"{_rimsApiBaseUrl.TrimEnd('/')}/{endpoint}?email={Uri.EscapeDataString(email)}";
            
            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.GetAsync(url, ct);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Не удалось подключиться к RIMS API. URL: {Url}", url);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Таймаут при подключении к RIMS API. URL: {Url}", url);
                return null;
            }
            
            if (response != null && !response.IsSuccessStatusCode)
            {
                _logger.LogWarning("RIMS API вернул ошибку: {StatusCode}", response.StatusCode);
                return null;
            }

            if (response == null)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var rimsResponse = JsonSerializer.Deserialize<RimsPersonResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (rimsResponse?.Status == true && rimsResponse.Data != null)
            {
                // Найти Entity с типом "Person"
                var personEntity = rimsResponse.Data.FirstOrDefault(e => e.Entity == "Person");
                return personEntity?.Result?.Items?.FirstOrDefault();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске пользователя в RIMS по Email: {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Синхронизация: создание/обновление пользователя из RIMS
    /// </summary>
    public async Task<User> SyncUserFromRimsAsync(string rimsUid, CancellationToken ct = default)
    {
        // 1. Получить данные из RIMS
        var rimsPerson = await FindPersonByUidAsync(rimsUid, ct);
        if (rimsPerson == null)
        {
            throw new InvalidOperationException($"Пользователь с RIMS Uid '{rimsUid}' не найден в RIMS");
        }

        // 2. Найти существующего пользователя по ExternalId
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalId == rimsUid, ct);

        // 3. Сопоставить Department (создать если нет)
        var department = await GetOrCreateDepartmentAsync(rimsPerson.Department, ct);

        // 4. Получить первый email из массива
        var email = rimsPerson.Emails?.FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("У пользователя из RIMS нет email адреса");
        }

        if (user == null)
        {
            // Создать нового пользователя
            user = new User
            {
                ExternalId = rimsUid,
                FullName = rimsPerson.Caption,
                Email = email,
                DepartmentId = department.DepartmentId,
                JobTitle = rimsPerson.JobTitle,
                HireDate = DateOnly.FromDateTime(DateTime.Now), // TODO: получить из RIMS если доступно
                OnboardingStatus = "Не начат",
                RimsLastSyncDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _logger.LogInformation("Создан новый пользователь из RIMS: {FullName} ({Uid})", user.FullName, rimsUid);
        }
        else
        {
            // Обновить существующего пользователя
            user.FullName = rimsPerson.Caption;
            if (!string.IsNullOrEmpty(email))
            {
                user.Email = email;
            }
            user.DepartmentId = department.DepartmentId;
            user.JobTitle = rimsPerson.JobTitle;
            user.RimsLastSyncDate = DateTime.UtcNow;

            _logger.LogInformation("Обновлён пользователь из RIMS: {FullName} ({Uid})", user.FullName, rimsUid);
        }

        await _context.SaveChangesAsync(ct);
        return user;
    }

    /// <summary>
    /// Получение или создание Department по названию из RIMS
    /// </summary>
    public async Task<Department> GetOrCreateDepartmentAsync(string rimsDepartmentName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rimsDepartmentName))
        {
            throw new ArgumentException("Название отдела не может быть пустым", nameof(rimsDepartmentName));
        }

        // 1. Искать по ExternalId (точное совпадение с RIMS)
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.ExternalId == rimsDepartmentName, ct);

        if (department != null)
        {
            return department;
        }

        // 2. Искать по названию (точное совпадение)
        department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == rimsDepartmentName, ct);

        if (department != null)
        {
            // Сохранить ExternalId для будущего сопоставления
            department.ExternalId = rimsDepartmentName;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Найден отдел по названию, сохранён ExternalId: {Name} -> {ExternalId}", 
                department.Name, rimsDepartmentName);
            return department;
        }

        // 3. Создать новый отдел
        department = new Department
        {
            Name = rimsDepartmentName,
            ExternalId = rimsDepartmentName
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Создан новый отдел из RIMS: {Name}", rimsDepartmentName);
        return department;
    }
}

