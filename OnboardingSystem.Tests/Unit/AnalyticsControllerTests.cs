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

public class AnalyticsControllerTests
{
    private readonly Mock<IProgressAnalyticsService> _analyticsServiceMock;
    private readonly Mock<ILearningPathService> _learningPathServiceMock;
    private readonly Mock<IDepartmentAnalyticsService> _departmentAnalyticsServiceMock;
    private readonly Mock<ILogger<AnalyticsController>> _loggerMock;
    private readonly AppDbContext _context;
    private readonly AnalyticsController _controller;

    public AnalyticsControllerTests()
    {
        _analyticsServiceMock = new Mock<IProgressAnalyticsService>();
        _learningPathServiceMock = new Mock<ILearningPathService>();
        _departmentAnalyticsServiceMock = new Mock<IDepartmentAnalyticsService>();
        _loggerMock = new Mock<ILogger<AnalyticsController>>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new AnalyticsController(
            _analyticsServiceMock.Object,
            _learningPathServiceMock.Object,
            _departmentAnalyticsServiceMock.Object,
            _context,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk_WithDashboardData()
    {
        var analytics = new AIAnalyticsDto
        {
            UserId = 10,
            AIInsight = "Хорошая прогрессия",
            EstimatedCompletionDays = 5
        };

        var learningPath = new LearningPathDto
        {
            UserId = 10,
            EstimatedCompletionDays = 5,
            PlannedOrder = { new PlannedModuleDto { ModuleId = 1, ModuleTitle = "Модуль 1" } }
        };

        _analyticsServiceMock
            .Setup(x => x.AnalyzeUserProgressAsync(10))
            .ReturnsAsync(analytics);

        _learningPathServiceMock
            .Setup(x => x.GenerateLearningPathAsync(10, "balanced", analytics))
            .ReturnsAsync(learningPath);

        var result = await _controller.GetDashboard(10);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AnalyticsDashboardDto>(okResult.Value);

        Assert.Equal(10, response.Analytics.UserId);
        Assert.Equal(1, response.LearningPath.PlannedOrder.Count);
        Assert.Equal(learningPath.PlannedOrder[0].ModuleId, response.NextModule?.ModuleId);
    }

    [Fact]
    public async Task GetLearningPath_ReturnsNotFound_WhenUserMissing()
    {
        _learningPathServiceMock
            .Setup(x => x.GenerateLearningPathAsync(123, It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Пользователь не найден"));

        var result = await _controller.GetLearningPath(123);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetDepartmentDashboard_ReturnsForbid_WhenUserFromAnotherDepartment()
    {
        var user = new User
        {
            UserId = 100,
            DepartmentId = 2,
            Email = "user@company.local",
            FullName = "User",
            OnboardingStatus = "Не начат"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", user.UserId.ToString())
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };

        var result = await _controller.GetDepartmentDashboard(1);

        Assert.IsType<ForbidResult>(result.Result);
    }
}
