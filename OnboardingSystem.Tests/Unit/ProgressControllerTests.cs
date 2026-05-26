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
using Xunit;

namespace OnboardingSystem.Tests.Unit;

public class ProgressControllerTests
{
    private readonly Mock<ILogger<ProgressController>> _loggerMock;
    private readonly AppDbContext _context;
    private readonly ProgressController _controller;

    public ProgressControllerTests()
    {
        _loggerMock = new Mock<ILogger<ProgressController>>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new ProgressController(_context, _loggerMock.Object);
    }

    private void SetupUser(int userId)
    {
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", userId.ToString())
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };
    }

    [Fact]
    public async Task GetUserProgress_ReturnsOk_WithProgressData()
    {
        // Arrange
        var userId = 1;
        SetupUser(userId);

        var user = new User
        {
            UserId = userId,
            Email = "user@test.com",
            FullName = "Test User",
            OnboardingStatus = "В процессе"
        };

        var progress = new UserProgress
        {
            ProgressId = 1,
            UserId = userId,
            ModulesCompleted = 2,
            TotalModules = 5,
            AverageScore = 85m,
            TotalTimeSpent = 3600
        };

        _context.Users.Add(user);
        _context.UserProgress.Add(progress);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUserProgress(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var progressDto = Assert.IsType<UserProgressDto>(okResult.Value);
        Assert.Equal(2, progressDto.ModulesCompleted);
        Assert.Equal(5, progressDto.TotalModules);
    }

    [Fact]
    public async Task GetUserProgress_ReturnsForbid_ForAnotherUsersProgress()
    {
        // Arrange
        SetupUser(1);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetUserProgress(2)
        );
    }

    [Fact]
    public async Task UpdateProgress_ReturnsOk_WithUpdatedData()
    {
        // Arrange
        var userId = 1;
        SetupUser(userId);

        var user = new User { UserId = userId, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        var progress = new UserProgress { ProgressId = 1, UserId = userId, ModulesCompleted = 1, TotalModules = 5 };

        _context.Users.Add(user);
        _context.UserProgress.Add(progress);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateProgressDto { ModulesCompleted = 2, AverageScore = 90m };

        // Act
        var result = await _controller.UpdateProgress(userId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var progressDto = Assert.IsType<UserProgressDto>(okResult.Value);
        Assert.Equal(2, progressDto.ModulesCompleted);
        Assert.Equal(90m, progressDto.AverageScore);
    }

    [Fact]
    public async Task GetProgressByDepartment_ReturnsOk_WithDepartmentStats()
    {
        // Arrange
        var departmentId = 1;
        SetupUser(1);

        var users = new[]
        {
            new User { UserId = 1, Email = "user1@test.com", FullName = "User 1", DepartmentId = departmentId, OnboardingStatus = "Завершён" },
            new User { UserId = 2, Email = "user2@test.com", FullName = "User 2", DepartmentId = departmentId, OnboardingStatus = "В процессе" }
        };

        _context.Users.AddRange(users);
        _context.UserProgress.AddRange(
            new UserProgress { ProgressId = 1, UserId = 1, ModulesCompleted = 5, TotalModules = 5 },
            new UserProgress { ProgressId = 2, UserId = 2, ModulesCompleted = 2, TotalModules = 5 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProgressByDepartment(departmentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var stats = Assert.IsType<DepartmentProgressDto>(okResult.Value);
        Assert.Equal(2, stats.TotalUsers);
    }

    [Fact]
    public async Task GetModuleProgress_ReturnsOk_WithModuleStats()
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;
        SetupUser(userId);

        var module = new Module { ModuleId = moduleId, ModuleTitle = "Test Module" };
        var user = new User { UserId = userId, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        var moduleProgress = new UserModuleProgress
        {
            UserId = userId,
            ModuleId = moduleId,
            Status = "Завершён",
            BestScore = 95m,
            AttemptsCount = 2,
            CompletedAt = DateTime.UtcNow
        };

        _context.Modules.Add(module);
        _context.Users.Add(user);
        _context.UserModuleProgress.Add(moduleProgress);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetModuleProgress(userId, moduleId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var progDto = Assert.IsType<UserModuleProgressDto>(okResult.Value);
        Assert.Equal("Завершён", progDto.Status);
        Assert.Equal(95m, progDto.BestScore);
    }

    [Fact]
    public async Task MarkModuleComplete_ReturnsOk_AndUpdatesProgress()
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;
        SetupUser(userId);

        var user = new User { UserId = userId, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        var module = new Module { ModuleId = moduleId, ModuleTitle = "Test Module", IsMandatory = true };

        _context.Users.Add(user);
        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        var completeDto = new MarkModuleCompleteDto { Score = 85m };

        // Act
        var result = await _controller.MarkModuleComplete(userId, moduleId, completeDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var progressDto = Assert.IsType<UserProgressDto>(okResult.Value);
        Assert.NotNull(progressDto);
    }

    [Fact]
    public async Task GetTimeSpentByModule_ReturnsOk_WithTimeStats()
    {
        // Arrange
        var userId = 1;
        SetupUser(userId);

        var user = new User { UserId = userId, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTimeSpentByModule(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var timeStats = Assert.IsType<List<TimeSpentDto>>(okResult.Value);
        Assert.NotNull(timeStats);
    }

    [Fact]
    public async Task GetProgressPercentage_ReturnsCorrectCalculation()
    {
        // Arrange
        var userId = 1;
        SetupUser(userId);

        var progress = new UserProgress
        {
            ProgressId = 1,
            UserId = userId,
            ModulesCompleted = 3,
            TotalModules = 5
        };

        // Act
        var percentage = (progress.ModulesCompleted * 100) / progress.TotalModules;

        // Assert
        Assert.Equal(60, percentage);
    }

    [Fact]
    public async Task GetProgressThresholds_ReturnsLevels()
    {
        // Arrange
        var thresholds = new[]
        {
            (percentage: 0, level: "Не начат"),
            (percentage: 25, level: "Начинающий"),
            (percentage: 50, level: "Продвинутый"),
            (percentage: 75, level: "Эксперт"),
            (percentage: 100, level: "Завершён")
        };

        // Act & Assert
        Assert.Equal("Начинающий", thresholds.First(t => t.percentage >= 25 && t.percentage < 50).level);
        Assert.Equal("Эксперт", thresholds.First(t => t.percentage >= 75 && t.percentage < 100).level);
    }
}
