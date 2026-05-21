using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OnboardingSystem.Controllers;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using OnboardingSystem.Services;
using OnboardingSystem.Tests.Fixtures;

namespace OnboardingSystem.Tests.Controllers;

public class TestAttemptsControllerTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<TestAttemptsController>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IGamificationService> _gamificationServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly TestAttemptsController _controller;

    public TestAttemptsControllerTests()
    {
        _fixture = new TestDatabaseFixture();
        _context = _fixture.Context;
        _loggerMock = new Mock<ILogger<TestAttemptsController>>();
        _emailServiceMock = new Mock<IEmailService>();
        _gamificationServiceMock = new Mock<IGamificationService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();

        _controller = new TestAttemptsController(
            _context,
            _loggerMock.Object,
            _emailServiceMock.Object,
            _gamificationServiceMock.Object,
            _notificationServiceMock.Object,
            _authorizationServiceMock.Object
        );

        // Устанавливаем user в контексте (имитация аутентификации)
        SetupControllerUser(null);
    }

    private void SetupControllerUser(User? user)
    {
        var claims = user != null
            ? new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.UserId.ToString())
            }
            : new List<System.Security.Claims.Claim>();

        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    public void Dispose()
    {
        _fixture?.Dispose();
    }

    // ============== ТЕСТЫ АВТОРИЗАЦИИ ==============

    /// <summary>
    /// Тест: неавторизованный пользователь не может сбросить попытки
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        SetupControllerUser(null); // Нет пользователя
        int userId = 4;
        int moduleId = 1;

        // Act
        var result = await _controller.ResetAttempts(userId, moduleId);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    /// <summary>
    /// Тест: обычный пользователь не может сбросить попытки
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldReturnForbid_WhenUserIsNotAdminOrHR()
    {
        // Arrange
        var regularUser = _context.Users.FirstOrDefault(u => u.UserId == 4); // Новый сотрудник
        SetupControllerUser(regularUser);

        _authorizationServiceMock.Setup(a => a.IsAdmin(It.IsAny<User>())).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsHr(It.IsAny<User>())).Returns(false);

        // Act
        var result = await _controller.ResetAttempts(4, 1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    /// <summary>
    /// Тест: администратор может сбросить попытки
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldSucceed_WhenUserIsAdmin()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1); // Администратор
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsHr(admin)).Returns(false);

        // Act
        var result = await _controller.ResetAttempts(4, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// Тест: HR-специалист может сбросить попытки
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldSucceed_WhenUserIsHR()
    {
        // Arrange
        var hr = _context.Users.FirstOrDefault(u => u.UserId == 2); // HR-специалист
        SetupControllerUser(hr);

        _authorizationServiceMock.Setup(a => a.IsAdmin(hr)).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsHr(hr)).Returns(true);

        // Act
        var result = await _controller.ResetAttempts(4, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    // ============== ТЕСТЫ БИЗНЕС-ЛОГИКИ ==============

    /// <summary>
    /// Тест: сброс попыток удаляет все попытки пользователя по модулю
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldDeleteAllAttempts_ForUserAndModule()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);

        var attemptsBeforeReset = _context.TestAttempts
            .Where(a => a.UserId == 4 && a.ModuleId == 1)
            .ToList();

        attemptsBeforeReset.Should().HaveCount(2); // Было 2 попытки

        // Act
        var result = await _controller.ResetAttempts(4, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        var attemptsAfterReset = _context.TestAttempts
            .Where(a => a.UserId == 4 && a.ModuleId == 1)
            .ToList();

        attemptsAfterReset.Should().BeEmpty(); // Попыток больше нет
    }

    /// <summary>
    /// Тест: сброс попыток сбрасывает статус прогресса на "В процессе"
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldResetProgressStatus_ToInProgress()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);

        var progressBefore = _context.UserModuleProgresses
            .FirstOrDefault(p => p.UserId == 4 && p.ModuleId == 1);

        progressBefore.Should().NotBeNull();
        progressBefore!.Status.Should().Be("Завершён");
        progressBefore.CompletionDate.Should().NotBeNull();

        // Act
        var result = await _controller.ResetAttempts(4, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        var progressAfter = _context.UserModuleProgresses
            .FirstOrDefault(p => p.UserId == 4 && p.ModuleId == 1);

        progressAfter.Should().NotBeNull();
        progressAfter!.Status.Should().Be("В процессе");
        progressAfter.CompletionDate.Should().BeNull();
    }

    /// <summary>
    /// Тест: сброс попыток логируется в ActionLog
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldCreateActionLog()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);

        var actionLogsBefore = _context.ActionLogs
            .Where(l => l.ActionType == "Администратор сбросил попытки теста")
            .ToList();

        // Act
        var result = await _controller.ResetAttempts(4, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        var actionLogsAfter = _context.ActionLogs
            .Where(l => l.ActionType == "Администратор сбросил попытки теста")
            .ToList();

        actionLogsAfter.Should().HaveCount(actionLogsBefore.Count + 1);

        var createdLog = actionLogsAfter.Last();
        createdLog.UserId.Should().Be(1); // Администратор
        createdLog.Details.Should().Contain("Сидоров Сергей"); // Имя сотрудника
        createdLog.Details.Should().Contain("Введение в компанию"); // Название модуля
    }

    /// <summary>
    /// Тест: возвращается ошибка, если пользователь не найден
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldReturnNotFound_WhenUserNotExists()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);

        int nonExistentUserId = 999;

        // Act
        var result = await _controller.ResetAttempts(nonExistentUserId, 1);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().BeOfType<object>(); // Возвращается объект с сообщением
    }

    /// <summary>
    /// Тест: возвращается ошибка, если модуль не найден
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldReturnNotFound_WhenModuleNotExists()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);

        int nonExistentModuleId = 999;

        // Act
        var result = await _controller.ResetAttempts(4, nonExistentModuleId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    /// <summary>
    /// Тест: сброс попыток не влияет на другие модули
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldNotAffectOtherModules()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);

        var attemptsOtherModuleBefore = _context.TestAttempts
            .Where(a => a.UserId == 4 && a.ModuleId == 2)
            .ToList();

        // Act
        await _controller.ResetAttempts(4, 1); // Сбрасываем модуль 1

        // Assert
        var attemptsOtherModuleAfter = _context.TestAttempts
            .Where(a => a.UserId == 4 && a.ModuleId == 2)
            .ToList();

        // Попытки модуля 2 должны остаться без изменений
        attemptsOtherModuleAfter.Should().HaveCount(attemptsOtherModuleBefore.Count);
    }

    /// <summary>
    /// Тест: сброс попыток не влияет на других пользователей
    /// </summary>
    [Fact]
    public async Task ResetAttempts_ShouldNotAffectOtherUsers()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);

        var attemptsOtherUserBefore = _context.TestAttempts
            .Where(a => a.UserId == 5 && a.ModuleId == 1)
            .ToList();

        // Act
        await _controller.ResetAttempts(4, 1); // Сбрасываем попытки пользователя 4

        // Assert
        var attemptsOtherUserAfter = _context.TestAttempts
            .Where(a => a.UserId == 5 && a.ModuleId == 1)
            .ToList();

        // Попытки пользователя 5 должны остаться без изменений
        attemptsOtherUserAfter.Should().HaveCount(attemptsOtherUserBefore.Count);
    }
}
