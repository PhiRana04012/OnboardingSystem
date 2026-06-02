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

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "1") }, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk_WhenAnalyticsAvailable()
    {
        var user = new User { UserId = 1, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var analytics = new AIAnalyticsDto { UserId = 1 };
        var learningPath = new LearningPathDto { UserId = 1, PlannedOrder = new List<PlannedModuleDto>() };

        _analyticsServiceMock.Setup(x => x.AnalyzeUserProgressAsync(It.Is<int>(id => id == 1), It.IsAny<CancellationToken>())).ReturnsAsync(analytics);
        _learningPathServiceMock.Setup(x => x.GenerateLearningPathAsync(
                It.Is<int>(id => id == 1),
                It.IsAny<string>(),
                It.IsAny<AIAnalyticsDto?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningPath);

        var result = await _controller.GetDashboard(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dashboard = Assert.IsType<AnalyticsDashboardDto>(okResult.Value);
        Assert.NotNull(dashboard.Analytics);
    }

    [Fact]
    public async Task GetDashboard_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _analyticsServiceMock.Setup(x => x.AnalyzeUserProgressAsync(It.Is<int>(id => id == 999), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentException("User not found"));

        var result = await _controller.GetDashboard(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetProgressAnalysis_ReturnsOk()
    {
        var analytics = new AIAnalyticsDto { UserId = 1 };
        _analyticsServiceMock.Setup(x => x.AnalyzeUserProgressAsync(It.Is<int>(id => id == 1), It.IsAny<CancellationToken>())).ReturnsAsync(analytics);

        var result = await _controller.GetProgressAnalysis(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAnalytics = Assert.IsType<AIAnalyticsDto>(okResult.Value);
        Assert.Equal(1, returnedAnalytics.UserId);
    }

    [Fact]
    public async Task GetProgressAnalysis_ReturnsNotFound_WhenNotFound()
    {
        _analyticsServiceMock.Setup(x => x.AnalyzeUserProgressAsync(It.Is<int>(id => id == 999), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentException("User not found"));

        var result = await _controller.GetProgressAnalysis(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
