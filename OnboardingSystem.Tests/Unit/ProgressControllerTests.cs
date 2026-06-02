using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OnboardingSystem.Controllers;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using OnboardingSystem.Services;
using Xunit;

namespace OnboardingSystem.Tests.Unit;

public class ProgressControllerTests
{
    private readonly Mock<ILogger<ProgressController>> _loggerMock;
    private readonly Mock<IGamificationService> _gamificationServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly AppDbContext _context;
    private readonly ProgressController _controller;

    public ProgressControllerTests()
    {
        _loggerMock = new Mock<ILogger<ProgressController>>();
        _gamificationServiceMock = new Mock<IGamificationService>();
        _notificationServiceMock = new Mock<INotificationService>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new ProgressController(
            _context,
            _loggerMock.Object,
            _gamificationServiceMock.Object,
            _notificationServiceMock.Object);

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "1") }, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };
    }

    [Fact]
    public async Task GetUserProgress_ReturnsOk_WithProgressData()
    {
        var department = new Department { DepartmentId = 1, Name = "IT" };
        var user = new User
        {
            UserId = 1,
            Email = "user@test.com",
            FullName = "Test User",
            DepartmentId = 1,
            OnboardingStatus = "В процессе"
        };

        var module = new Module { ModuleId = 1, Title = "Test Module", IsMandatory = true };

        _context.Departments.Add(department);
        _context.Users.Add(user);
        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        var result = await _controller.GetUserProgress(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedData = Assert.IsType<UserOnboardingProgressDto>(okResult.Value);
        Assert.Equal(1, returnedData.UserId);
    }

    [Fact]
    public async Task GetUserProgress_ReturnsNotFound_ForNonexistentUser()
    {
        var result = await _controller.GetUserProgress(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetModuleProgress_ReturnsOk()
    {
        var user = new User { UserId = 1, Email = "user@test.com", FullName = "Test User", OnboardingStatus = "В процессе" };
        var module = new Module { ModuleId = 1, Title = "Test Module", IsMandatory = true };
        var progress = new UserModuleProgress { UserId = 1, ModuleId = 1, Status = "Завершён", StartDate = DateTime.UtcNow };

        _context.Users.Add(user);
        _context.Modules.Add(module);
        _context.UserModuleProgresses.Add(progress);
        await _context.SaveChangesAsync();

        var result = await _controller.GetModuleProgress(1, 1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var progressDto = Assert.IsType<UserModuleProgressDto>(okResult.Value);
        Assert.Equal(1, progressDto.ModuleId);
    }

    [Fact]
    public async Task GetModuleProgress_ReturnsNotFound_WhenProgressDoesNotExist()
    {
        var result = await _controller.GetModuleProgress(1, 999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task MarkModuleAsRead_ReturnsOk()
    {
        var user = new User { UserId = 1, Email = "user@test.com", FullName = "Test User", OnboardingStatus = "В процессе" };
        var module = new Module { ModuleId = 1, Title = "Test Module", IsMandatory = true };

        _context.Users.Add(user);
        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        var markDto = new MarkModuleAsReadDto { ModuleId = 1 };
        var result = await _controller.MarkModuleAsRead(1, markDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAllProgress_ReturnsOkWithEmptyList()
    {
        var result = await _controller.GetAllProgress(null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var progressList = Assert.IsType<List<UserModuleProgressDto>>(okResult.Value);
        Assert.NotNull(progressList);
    }
}
