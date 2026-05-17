using OnboardingSystem.Data;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Services;

/// <summary>
/// Сервис для проверки доступа пользователей к ресурсам
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Проверяет, является ли пользователь начальником отдела
    /// </summary>
    bool IsDepartmentHead(User user);

    /// <summary>
    /// Проверяет, может ли пользователь управлять наставниками в отделе
    /// </summary>
    bool CanManageMentees(User user, int departmentId);

    /// <summary>
    /// Проверяет, может ли пользователь просматривать отчёты отдела
    /// </summary>
    bool CanViewDepartmentReports(User user, int departmentId);

    /// <summary>
    /// Проверяет, может ли пользователь просматривать данные пользователя
    /// </summary>
    bool CanViewUser(User currentUser, User targetUser);

    /// <summary>
    /// Проверяет, может ли пользователь редактировать данные пользователя
    /// </summary>
    bool CanEditUser(User currentUser, User targetUser);

    /// <summary>
    /// Получает должность пользователя
    /// </summary>
    string? GetUserJobTitle(User user);

    /// <summary>
    /// Получает роли пользователя
    /// </summary>
    List<string> GetUserRoles(User user);

    /// <summary>
    /// Проверяет, имеет ли пользователь роль
    /// </summary>
    bool HasRole(User user, string roleName);

    bool IsAdmin(User user);

    bool IsHr(User user);

    /// <summary>
    /// Админ/HR — любой отдел; начальник отдела — только свой
    /// </summary>
    bool CanManageDepartment(User user, int? departmentId);

    /// <summary>
    /// Управление модулями: админ/HR — все; начальник — только модули своего отдела
    /// </summary>
    bool CanManageModule(User user, Module module);

    bool CanCreateOrDeleteUser(User user);
}

public class AuthorizationService : IAuthorizationService
{
    private readonly AppDbContext _context;
    private const string DepartmentHeadJobTitle = "Начальник отдела";
    private const string AdminRole = "Администратор системы";

    public AuthorizationService(AppDbContext context)
    {
        _context = context;
    }

    public bool IsDepartmentHead(User user)
    {
        if (user == null) return false;
        if (HasRole(user, AdminRole)) return false;
        return user.JobTitle?.Title == DepartmentHeadJobTitle;
    }

    public bool CanManageMentees(User currentUser, int departmentId)
    {
        if (currentUser == null) return false;
        
        // Администратор может управлять всем
        if (HasRole(currentUser, AdminRole)) return true;
        
        // Начальник отдела может управлять только своим отделом
        if (IsDepartmentHead(currentUser) && currentUser.DepartmentId == departmentId) return true;
        
        return false;
    }

    public bool CanViewDepartmentReports(User currentUser, int departmentId)
    {
        if (currentUser == null) return false;
        
        // Администратор может видеть всё
        if (HasRole(currentUser, AdminRole)) return true;
        
        // HR специалист может видеть отчёты
        if (HasRole(currentUser, "HR-специалист")) return true;
        
        // Начальник отдела может видеть отчёты своего отдела
        if (IsDepartmentHead(currentUser) && currentUser.DepartmentId == departmentId) return true;
        
        return false;
    }

    public bool CanViewUser(User currentUser, User targetUser)
    {
        if (currentUser == null || targetUser == null) return false;
        if (currentUser.UserId == targetUser.UserId) return true; // Сам себя может видеть
        
        // Администратор может видеть всех
        if (HasRole(currentUser, AdminRole)) return true;
        
        // Наставник может видеть своих подопечных
        if (currentUser.InverseMentor?.Any(u => u.UserId == targetUser.UserId) ?? false) return true;
        
        // Начальник отдела может видеть пользователей своего отдела
        if (IsDepartmentHead(currentUser) && currentUser.DepartmentId == targetUser.DepartmentId) return true;
        
        return false;
    }

    public bool CanEditUser(User currentUser, User targetUser)
    {
        if (currentUser == null || targetUser == null) return false;
        if (currentUser.UserId == targetUser.UserId) return true; // Сам себя может редактировать
        
        // Только администратор может редактировать других пользователей
        if (HasRole(currentUser, AdminRole)) return true;
        
        // Начальник отдела может редактировать пользователей своего отдела (кроме админов)
        if (IsDepartmentHead(currentUser) && currentUser.DepartmentId == targetUser.DepartmentId)
        {
            if (HasRole(targetUser, AdminRole)) return false;
            return true;
        }
        
        return false;
    }

    public string? GetUserJobTitle(User user)
    {
        return user?.JobTitle?.Title;
    }

    public List<string> GetUserRoles(User user)
    {
        if (user == null) return new();
        return user.Roles?.Select(r => r.RoleName).ToList() ?? new();
    }

    public bool HasRole(User user, string roleName)
    {
        if (user == null) return false;
        return user.Roles?.Any(r => r.RoleName == roleName) ?? false;
    }

    public bool IsAdmin(User user) => HasRole(user, AdminRole);

    public bool IsHr(User user) => HasRole(user, "HR-специалист");

    public bool CanManageDepartment(User user, int? departmentId)
    {
        if (user == null) return false;
        if (IsAdmin(user) || IsHr(user)) return true;
        if (!IsDepartmentHead(user) || !departmentId.HasValue) return false;
        return user.DepartmentId == departmentId.Value;
    }

    public bool CanManageModule(User user, Module module)
    {
        if (user == null || module == null) return false;
        if (IsAdmin(user) || IsHr(user)) return true;

        if (!IsDepartmentHead(user)) return false;

        var departmentIds = module.ModuleDepartments?
            .Select(md => md.DepartmentId)
            .Distinct()
            .ToList() ?? new List<int>();

        if (module.DepartmentId.HasValue && !departmentIds.Contains(module.DepartmentId.Value))
            departmentIds.Add(module.DepartmentId.Value);

        if (departmentIds.Count == 0) return false;

        return departmentIds.Count == 1 && departmentIds[0] == user.DepartmentId;
    }

    public bool CanCreateOrDeleteUser(User user)
    {
        if (user == null) return false;
        return IsAdmin(user) || IsHr(user);
    }
}
