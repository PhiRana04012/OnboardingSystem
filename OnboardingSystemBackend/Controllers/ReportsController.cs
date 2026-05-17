using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Services;
using IAppAuthorizationService = OnboardingSystem.Services.IAuthorizationService;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportsController> _logger;
    private readonly IReportExportService _exportService;
    private readonly IAppAuthorizationService _authorizationService;

    public ReportsController(AppDbContext context, ILogger<ReportsController> logger, IReportExportService exportService, IAppAuthorizationService authorizationService)
    {
        _context = context;
        _logger = logger;
        _exportService = exportService;
        _authorizationService = authorizationService;
    }


    /// <summary>
    /// Получить отчёт о прогрессе онбординга сотрудника
    /// </summary>
    [HttpGet("onboarding-progress/{userId}")]
    [ProducesResponseType(typeof(OnboardingProgressReportDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OnboardingProgressReportDto>> GetOnboardingProgressReport(int userId)
    {
        var currentUser = await this.GetCurrentUserAsync(_context);
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Mentor)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound();
        }

        // Проверяем доступ
        if (currentUser != null && !_authorizationService.CanViewDepartmentReports(currentUser, user.DepartmentId))
        {
            return Forbid();
        }

        // Получаем модули для пользователя
        var userModules = await _context.Modules
            .Where(m =>
                (m.DepartmentId == null && !m.ModuleDepartments.Any()) ||
                m.DepartmentId == user.DepartmentId ||
                m.ModuleDepartments.Any(md => md.DepartmentId == user.DepartmentId))
            .ToListAsync();

        var progressList = await _context.UserModuleProgresses
            .Include(p => p.Module)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        var attempts = await _context.TestAttempts
            .Where(t => t.UserId == userId)
            .GroupBy(t => t.ModuleId)
            .Select(g => new
            {
                ModuleId = g.Key,
                AttemptsCount = g.Count(),
                BestScore = g.Max(t => t.Score),
                IsPassed = g.Any(t => t.IsPassed)
            })
            .ToDictionaryAsync(x => x.ModuleId, x => x);

        var moduleStatuses = userModules.Select(module =>
        {
            var progress = progressList.FirstOrDefault(p => p.ModuleId == module.ModuleId);
            var attemptInfo = attempts.GetValueOrDefault(module.ModuleId);

            return new ModuleStatusDto
            {
                ModuleId = module.ModuleId,
                ModuleTitle = module.Title,
                IsMandatory = module.IsMandatory,
                Status = progress?.Status ?? "Не начат",
                StartDate = progress?.StartDate,
                CompletionDate = progress?.CompletionDate,
                AttemptsCount = attemptInfo?.AttemptsCount ?? 0,
                BestScore = attemptInfo?.BestScore,
                IsPassed = attemptInfo?.IsPassed ?? false
            };
        }).ToList();

        var mandatoryModules = userModules.Where(m => m.IsMandatory).ToList();
        var completedMandatory = moduleStatuses.Count(m => m.IsMandatory && m.Status == "Завершён");
        var progressPercentage = mandatoryModules.Count > 0
            ? (decimal)completedMandatory / mandatoryModules.Count * 100
            : 0;

        var firstStartDate = progressList
            .Where(p => p.StartDate.HasValue)
            .Select(p => p.StartDate!.Value)
            .DefaultIfEmpty()
            .Min();

        var lastCompletionDate = progressList
            .Where(p => p.CompletionDate.HasValue)
            .Select(p => p.CompletionDate!.Value)
            .DefaultIfEmpty()
            .Max();

        var report = new OnboardingProgressReportDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            DepartmentName = user.Department.Name,
            MentorName = user.Mentor != null ? user.Mentor.FullName : null,
            HireDate = user.HireDate,
            OnboardingStatus = user.OnboardingStatus,
            OnboardingStartDate = firstStartDate != default ? firstStartDate : null,
            OnboardingCompletionDate = lastCompletionDate != default ? lastCompletionDate : null,
            ProgressPercentage = progressPercentage,
            TotalMandatoryModules = mandatoryModules.Count,
            CompletedMandatoryModules = completedMandatory,
            ModuleStatuses = moduleStatuses
        };

        return Ok(report);
    }

    /// <summary>
    /// Получить отчёт о результатах тестов
    /// </summary>
    [HttpGet("test-results")]
    [ProducesResponseType(typeof(List<TestResultsReportDto>), 200)]
    public async Task<ActionResult<List<TestResultsReportDto>>> GetTestResultsReport([FromQuery] int? userId, [FromQuery] int? moduleId)
    {
        var currentUser = await this.GetCurrentUserAsync(_context);

        if (userId.HasValue)
        {
            var targetUser = await _context.Users.FindAsync(userId.Value);
            if (targetUser == null) return NotFound();
            if (currentUser != null && !_authorizationService.CanViewDepartmentReports(currentUser, targetUser.DepartmentId))
            {
                return Forbid();
            }
        }

        var query = _context.TestAttempts
            .Include(t => t.User)
            .Include(t => t.Module)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        if (moduleId.HasValue)
        {
            query = query.Where(t => t.ModuleId == moduleId.Value);
        }

        if (currentUser != null
            && _authorizationService.IsDepartmentHead(currentUser)
            && !_authorizationService.IsAdmin(currentUser)
            && !_authorizationService.IsHr(currentUser))
        {
            query = query.Where(t => t.User.DepartmentId == currentUser.DepartmentId);
        }

        var attempts = await query
            .OrderByDescending(t => t.AttemptDate)
            .ToListAsync();

        var result = attempts.Select(t =>
        {
            // Вычисляем количество правильных ответов (примерно, на основе балла)
            // В реальной системе это должно храниться в базе данных
            var totalQuestions = 10; // По умолчанию, можно улучшить
            var correctAnswers = (int)Math.Round(t.Score / 100 * totalQuestions);

            return new TestResultsReportDto
            {
                UserId = t.UserId,
                FullName = t.User.FullName,
                Email = t.User.Email,
                ModuleId = t.ModuleId,
                ModuleTitle = t.Module.Title,
                AttemptDate = t.AttemptDate,
                AttemptNumber = t.AttemptNumber,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                Score = t.Score,
                IsPassed = t.IsPassed
            };
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Получить отчёт по подразделению
    /// </summary>
    [HttpGet("department/{departmentId}")]
    [ProducesResponseType(typeof(DepartmentReportDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DepartmentReportDto>> GetDepartmentReport(int departmentId)
    {
        var currentUser = await this.GetCurrentUserAsync(_context);
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);

        if (department == null)
        {
            return NotFound();
        }

        if (currentUser != null && !_authorizationService.CanViewDepartmentReports(currentUser, departmentId))
        {
            return Forbid();
        }

        var users = await _context.Users
            .Where(u => u.DepartmentId == departmentId)
            .ToListAsync();

        var userIds = users.Select(u => u.UserId).ToList();

        var userModules = await _context.Modules
            .Where(m =>
                (m.DepartmentId == null && !m.ModuleDepartments.Any()) ||
                m.DepartmentId == departmentId ||
                m.ModuleDepartments.Any(md => md.DepartmentId == departmentId))
            .ToListAsync();

        var progressList = await _context.UserModuleProgresses
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync();

        var userProgressSummaries = new List<UserProgressSummaryDto>();

        foreach (var user in users)
        {
            var userModuleProgress = progressList
                .Where(p => p.UserId == user.UserId)
                .ToList();

            var mandatoryModules = userModules.Where(m => m.IsMandatory).ToList();
            var completedMandatory = userModuleProgress
                .Count(p => mandatoryModules.Any(m => m.ModuleId == p.ModuleId) && p.Status == "Завершён");

            var progressPercentage = mandatoryModules.Count > 0
                ? (decimal)completedMandatory / mandatoryModules.Count * 100
                : 0;

            var completionDate = userModuleProgress
                .Where(p => p.CompletionDate.HasValue)
                .Select(p => p.CompletionDate!.Value)
                .DefaultIfEmpty()
                .Max();

            userProgressSummaries.Add(new UserProgressSummaryDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                OnboardingStatus = user.OnboardingStatus,
                ProgressPercentage = progressPercentage,
                CompletionDate = completionDate != default ? completionDate : null
            });
        }

        var totalUsers = users.Count;
        var usersInProgress = users.Count(u => u.OnboardingStatus == "В процессе");
        var usersCompleted = users.Count(u => u.OnboardingStatus == "Завершён");
        var usersNotStarted = users.Count(u => u.OnboardingStatus == "Не начат");

        var averageProgress = userProgressSummaries.Any()
            ? userProgressSummaries.Average(u => u.ProgressPercentage)
            : 0;

        var report = new DepartmentReportDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentName = department.Name,
            TotalUsers = totalUsers,
            UsersInProgress = usersInProgress,
            UsersCompleted = usersCompleted,
            UsersNotStarted = usersNotStarted,
            AverageProgressPercentage = averageProgress,
            Users = userProgressSummaries
        };

        return Ok(report);
    }

    [HttpGet("onboarding-progress/{userId}/export")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ExportOnboardingProgressList(int userId, [FromQuery] string format = "excel")
    {
        var result = await GetOnboardingProgressReport(userId);
        if (result.Result is NotFoundResult) return NotFound();
        var report = ((OkObjectResult)result.Result).Value as OnboardingProgressReportDto;

        if (format.ToLower() == "pdf")
        {
            var pdfBytes = _exportService.ExportOnboardingProgressToPdf(report);
            return File(pdfBytes, "application/pdf", $"Progress_{report.FullName}.pdf");
        }
        
        var excelBytes = _exportService.ExportOnboardingProgressToExcel(report);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Progress_{report.FullName}.xlsx");
    }

    [HttpGet("test-results/export")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ExportTestResultsList([FromQuery] int? userId, [FromQuery] int? moduleId, [FromQuery] string format = "excel")
    {
        var result = await GetTestResultsReport(userId, moduleId);
        var report = ((OkObjectResult)result.Result).Value as List<TestResultsReportDto>;

        if (format.ToLower() == "pdf")
        {
            var pdfBytes = _exportService.ExportTestResultsToPdf(report);
            return File(pdfBytes, "application/pdf", $"TestResults.pdf");
        }
        
        var excelBytes = _exportService.ExportTestResultsToExcel(report);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TestResults.xlsx");
    }

    [HttpGet("department/{departmentId}/export")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ExportDepartmentReportList(int departmentId, [FromQuery] string format = "excel")
    {
        var result = await GetDepartmentReport(departmentId);
        if (result.Result is NotFoundResult) return NotFound();
        var report = ((OkObjectResult)result.Result).Value as DepartmentReportDto;

        if (format.ToLower() == "pdf")
        {
            var pdfBytes = _exportService.ExportDepartmentReportToPdf(report);
            return File(pdfBytes, "application/pdf", $"Department_{report.DepartmentName}.pdf");
        }
        
        var excelBytes = _exportService.ExportDepartmentReportToExcel(report);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Department_{report.DepartmentName}.xlsx");
    }
}


