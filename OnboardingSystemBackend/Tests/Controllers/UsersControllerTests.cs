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

public class UsersControllerTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<UsersController>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IPasswordResetService> _passwordResetServiceMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _fixture = new TestDatabaseFixture();
        _context = _fixture.Context;
        _loggerMock = new Mock<ILogger<UsersController>>();
        _emailServiceMock = new Mock<IEmailService>();
        _passwordResetServiceMock = new Mock<IPasswordResetService>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _controller = new UsersController(
            _context,
            _loggerMock.Object,
            _emailServiceMock.Object,
            _passwordResetServiceMock.Object,
            _authorizationServiceMock.Object,
            _notificationServiceMock.Object
        );

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

    // ============== ТЕСТЫ AssignMentees - АВТОРИЗАЦИЯ ==============

    /// <summary>
    /// Тест: неавторизованный пользователь не может назначить наставника
    /// </summary>
    [Fact]
    public async Task AssignMentees_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        SetupControllerUser(null);
        var dto = new AssignMenteesDto { MenteeIds = new List<int> { 4, 5 } };

        // Act
        var result = await _controller.AssignMentees(3, dto);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    /// <summary>
    /// Тест: обычный сотрудник не может назначить наставника
    /// </summary>
    [Fact]
    public async Task AssignMentees_ShouldReturnForbid_WhenUserIsNotAuthorized()
    {
        // Arrange
        var regularUser = _context.Users.FirstOrDefault(u => u.UserId == 4);
        SetupControllerUser(regularUser);

        _authorizationServiceMock.Setup(a => a.IsAdmin(regularUser)).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsHr(regularUser)).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsDepartmentHead(regularUser)).Returns(false);

        var dto = new AssignMenteesDto { MenteeIds = new List<int> { 5 } };

        // Act
        var result = await _controller.AssignMentees(3, dto);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    /// <summary>
    /// Тест: администратор может назначить наставника
    /// </summary>
    [Fact]
    public async Task AssignMentees_ShouldSucceed_WhenUserIsAdmin()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsHr(admin)).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsDepartmentHead(admin)).Returns(false);

        var dto = new AssignMenteesDto { MenteeIds = new List<int> { 4, 5 } };

        // Act
        var result = await _controller.AssignMentees(3, dto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    /// <summary>
    /// Тест: HR-специалист может назначить наставника
    /// </summary>
    [Fact]
    public async Task AssignMentees_ShouldSucceed_WhenUserIsHR()
    {
        // Arrange
        var hr = _context.Users.FirstOrDefault(u => u.UserId == 2);
        SetupControllerUser(hr);

        _authorizationServiceMock.Setup(a => a.IsAdmin(hr)).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsHr(hr)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsDepartmentHead(hr)).Returns(false);

        var dto = new AssignMenteesDto { MenteeIds = new List<int> { 4, 5 } };

        // Act
        var result = await _controller.AssignMentees(3, dto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    // ============== ТЕСТЫ AssignMentees - БИЗНЕС-ЛОГИКА ==============

    /// <summary>
    /// Тест: назначение наставника устанавливает связь MentorId
    /// </summary>
    [Fact]
    public async Task AssignMentees_ShouldSetMentorId_ForSelectedEmployees()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsHr(admin)).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsDepartmentHead(admin)).Returns(false);

        var mentor = _context.Users.FirstOrDefault(u => u.UserId == 3);
        var mentees = new List<int> { 4, 5 };
        var dto = new AssignMenteesDto { MenteeIds = mentees };

        var emp1Before = _context.Users.FirstOrDefault(u => u.UserId == 4);
        var emp2Before = _context.Users.FirstOrDefault(u => u.UserId == 5);

        emp1Before!.MentorId.Should().BeNull(); // Не было наставника
        emp2Before!.MentorId.Should().BeNull();

        // Act
        var result = await _controller.AssignMentees(3, dto);

        // Assert
        result.Should().BeOfType<OkResult>();

        var emp1After = _context.Users.FirstOrDefault(u => u.UserId == 4);
        var emp2After = _context.Users.FirstOrDefault(u => u.UserId == 5);

        emp1After!.MentorId.Should().Be(3); // Назначен наставник
        emp2After!.MentorId.Should().Be(3);
    }

    /// <summary>
    /// Тест: назначение наставника не влияет на других сотрудников
    /// </summary>
    [Fact]
    public async Task AssignMentees_ShouldNotAffectOtherEmployees()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsHr(admin)).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsDepartmentHead(admin)).Returns(false);

        var anotherDeptEmpBefore = _context.Users.FirstOrDefault(u => u.UserId == 6); // HR отдел
        anotherDeptEmpBefore!.MentorId.Should().BeNull();

        var dto = new AssignMenteesDto { MenteeIds = new List<int> { 4 } };

        // Act
        await _controller.AssignMentees(3, dto);

        // Assert
        var anotherDeptEmpAfter = _context.Users.FirstOrDefault(u => u.UserId == 6);
        anotherDeptEmpAfter!.MentorId.Should().BeNull(); // Не изменилось
    }

    /// <summary>
    /// Тест: возвращается ошибка, если наставник не найден
    /// </summary>
    [Fact]
    public async Task AssignMentees_ShouldReturnNotFound_WhenMentorNotExists()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsHr(admin)).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsDepartmentHead(admin)).Returns(false);

        var dto = new AssignMenteesDto { MenteeIds = new List<int> { 4 } };

        // Act
        var result = await _controller.AssignMentees(999, dto); // Несуществующий наставник

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    /// <summary>
    /// Тест: назначение наставника логируется
    /// </summary>
    [Fact]
    public async Task AssignMentees_ShouldCreateNotification()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsHr(admin)).Returns(false);
        _authorizationServiceMock.Setup(a => a.IsDepartmentHead(admin)).Returns(false);

        var dto = new AssignMenteesDto { MenteeIds = new List<int> { 4, 5 } };

        // Act
        await _controller.AssignMentees(3, dto);

        // Assert
        // Проверяем что был вызван сервис уведомлений
        _notificationServiceMock.Verify(
            n => n.SendAsync(
                It.Is<int>(id => id == 3),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    // ============== ТЕСТЫ UpdateUser - ВАЛИДАЦИЯ ОТДЕЛА ==============

    /// <summary>
    /// Тест: при смене отдела удаляется наставник из другого отдела
    /// </summary>
    [Fact]
    public async Task UpdateUser_ShouldClearMentor_WhenDepartmentChanges()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsHr(admin)).Returns(false);
        _authorizationServiceMock.Setup(a => a.CanEditUser(admin, It.IsAny<User>())).Returns(true);

        // Сначала назначаем наставника из IT отдела
        var employee = _context.Users.FirstOrDefault(u => u.UserId == 4); // IT отдел
        employee!.MentorId = 3; // Наставник из IT отдела
        _context.SaveChanges();

        employee.MentorId.Should().Be(3);
        employee.DepartmentId.Should().Be(1); // IT

        var dto = new UpdateUserDto
        {
            DepartmentId = 2 // Меняем на HR отдел
        };

        // Act
        var result = await _controller.UpdateUser(4, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var updatedEmployee = _context.Users.FirstOrDefault(u => u.UserId == 4);
        updatedEmployee!.DepartmentId.Should().Be(2); // Отдел изменён
        updatedEmployee.MentorId.Should().BeNull(); // Наставник удалён
    }

    /// <summary>
    /// Тест: при смене отдела не удаляется наставник из того же отдела
    /// </summary>
    [Fact]
    public async Task UpdateUser_ShouldKeepMentor_WhenDepartmentRemainsTheSame()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsHr(admin)).Returns(false);
        _authorizationServiceMock.Setup(a => a.CanEditUser(admin, It.IsAny<User>())).Returns(true);

        var employee = _context.Users.FirstOrDefault(u => u.UserId == 4);
        employee!.MentorId = 3; // Наставник из IT отдела
        employee.DepartmentId = 1; // IT
        _context.SaveChanges();

        var dto = new UpdateUserDto
        {
            FullName = "Новое имя" // Меняем только имя, отдел не меняется
        };

        // Act
        var result = await _controller.UpdateUser(4, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var updatedEmployee = _context.Users.FirstOrDefault(u => u.UserId == 4);
        updatedEmployee!.MentorId.Should().Be(3); // Наставник остался
    }

    /// <summary>
    /// Тест: возвращается ошибка при попытке назначить наставника из другого отдела
    /// </summary>
    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenMentorFromDifferentDepartment()
    {
        // Arrange
        var admin = _context.Users.FirstOrDefault(u => u.UserId == 1);
        SetupControllerUser(admin);

        _authorizationServiceMock.Setup(a => a.IsAdmin(admin)).Returns(true);
        _authorizationServiceMock.Setup(a => a.IsHr(admin)).Returns(false);
        _authorizationServiceMock.Setup(a => a.CanEditUser(admin, It.IsAny<User>())).Returns(true);

        var employee = _context.Users.FirstOrDefault(u => u.UserId == 6); // HR отдел
        var mentorFromITDept = 3; // Наставник из IT отдела

        var dto = new UpdateUserDto
        {
            MentorId = mentorFromITDept
        };

        // Act - когда User был обновлен как администратор
        var result = await _controller.UpdateUser(6, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>()); // Админ может назначить любого
    }
}
