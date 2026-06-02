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

public class TestAttemptsControllerTests
{
    private readonly Mock<ILogger<TestAttemptsController>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IGamificationService> _gamificationServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly AppDbContext _context;
    private readonly TestAttemptsController _controller;

    public TestAttemptsControllerTests()
    {
        _loggerMock = new Mock<ILogger<TestAttemptsController>>();
        _emailServiceMock = new Mock<IEmailService>();
        _gamificationServiceMock = new Mock<IGamificationService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new TestAttemptsController(
            _context,
            _loggerMock.Object,
            _emailServiceMock.Object,
            _gamificationServiceMock.Object,
            _notificationServiceMock.Object,
            _authorizationServiceMock.Object);

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "1") }, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };
    }

    [Fact]
    public async Task GetUserAttempts_ReturnsOk()
    {
        var user = new User { UserId = 1, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        var module = new Module { ModuleId = 1, Title = "Test Module", IsMandatory = true };

        var attempts = new[]
        {
            new TestAttempt { AttemptId = 1, UserId = 1, ModuleId = 1, AttemptDate = DateTime.UtcNow, AttemptNumber = 1, Score = 85, IsPassed = true },
            new TestAttempt { AttemptId = 2, UserId = 1, ModuleId = 1, AttemptDate = DateTime.UtcNow.AddDays(-1), AttemptNumber = 2, Score = 90, IsPassed = true }
        };

        _context.Users.Add(user);
        _context.Modules.Add(module);
        _context.TestAttempts.AddRange(attempts);
        await _context.SaveChangesAsync();

        var result = await _controller.GetUserAttempts(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAttempts = Assert.IsType<List<TestAttemptDto>>(okResult.Value);
        Assert.Equal(2, returnedAttempts.Count);
    }

    [Fact]
    public async Task GetUserAttempts_ReturnsEmptyList_WhenUserHasNoAttempts()
    {
        var result = await _controller.GetUserAttempts(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAttempts = Assert.IsType<List<TestAttemptDto>>(okResult.Value);
        Assert.Empty(returnedAttempts);
    }

    [Fact]
    public async Task GetUserAttempts_OrderedByDateDescending()
    {
        var user = new User { UserId = 1, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        var module = new Module { ModuleId = 1, Title = "Test Module", IsMandatory = true };

        _context.Users.Add(user);
        _context.Modules.Add(module);
        _context.TestAttempts.AddRange(
            new TestAttempt { AttemptId = 1, UserId = 1, ModuleId = 1, AttemptDate = DateTime.UtcNow.AddDays(-2), AttemptNumber = 1, Score = 70, IsPassed = false },
            new TestAttempt { AttemptId = 2, UserId = 1, ModuleId = 1, AttemptDate = DateTime.UtcNow, AttemptNumber = 2, Score = 85, IsPassed = true }
        );
        await _context.SaveChangesAsync();

        var result = await _controller.GetUserAttempts(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAttempts = Assert.IsType<List<TestAttemptDto>>(okResult.Value);
        Assert.Equal(2, returnedAttempts[0].AttemptNumber);
    }

    [Fact]
    public async Task GetModuleAttempts_ReturnsOk()
    {
        var users = new[]
        {
            new User { UserId = 1, Email = "user1@test.com", FullName = "User 1", OnboardingStatus = "В процессе" },
            new User { UserId = 2, Email = "user2@test.com", FullName = "User 2", OnboardingStatus = "В процессе" }
        };

        var module = new Module { ModuleId = 1, Title = "Test Module", IsMandatory = true };

        var attempts = new[]
        {
            new TestAttempt { AttemptId = 1, UserId = 1, ModuleId = 1, AttemptDate = DateTime.UtcNow, AttemptNumber = 1, Score = 80, IsPassed = true },
            new TestAttempt { AttemptId = 2, UserId = 2, ModuleId = 1, AttemptDate = DateTime.UtcNow, AttemptNumber = 1, Score = 75, IsPassed = true },
            new TestAttempt { AttemptId = 3, UserId = 1, ModuleId = 2, AttemptDate = DateTime.UtcNow, AttemptNumber = 1, Score = 90, IsPassed = true }
        };

        _context.Users.AddRange(users);
        _context.Modules.Add(module);
        _context.TestAttempts.AddRange(attempts);
        await _context.SaveChangesAsync();

        var result = await _controller.GetModuleAttempts(1, 1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAttempts = Assert.IsType<List<TestAttemptDto>>(okResult.Value);
        Assert.Equal(1, returnedAttempts.Count);
        Assert.All(returnedAttempts, a => Assert.Equal(1, a.ModuleId));
    }

    [Fact]
    public async Task GetAttempt_ReturnsOk_WhenAttemptExists()
    {
        var user = new User { UserId = 1, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        var module = new Module { ModuleId = 1, Title = "Test Module", IsMandatory = true };
        var attempt = new TestAttempt
        {
            AttemptId = 1,
            UserId = 1,
            ModuleId = 1,
            AttemptDate = DateTime.UtcNow,
            AttemptNumber = 1,
            Score = 100,
            IsPassed = true
        };

        _context.Users.Add(user);
        _context.Modules.Add(module);
        _context.TestAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAttempt(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAttempt = Assert.IsType<TestAttemptDto>(okResult.Value);
        Assert.Equal(1, returnedAttempt.AttemptId);
    }

    [Fact]
    public async Task GetAttempt_ReturnsNotFound_WhenAttemptDoesNotExist()
    {
        var result = await _controller.GetAttempt(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
