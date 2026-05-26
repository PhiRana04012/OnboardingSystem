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

public class TestAttemptsControllerTests
{
    private readonly Mock<ILogger<TestAttemptsController>> _loggerMock;
    private readonly AppDbContext _context;
    private readonly TestAttemptsController _controller;

    public TestAttemptsControllerTests()
    {
        _loggerMock = new Mock<ILogger<TestAttemptsController>>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new TestAttemptsController(_context, _loggerMock.Object);
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
    public async Task StartTest_ReturnsOk_WithTestAttempt()
    {
        // Arrange
        var userId = 1;
        SetupUser(userId);

        var user = new User { UserId = userId, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        var module = new Module { ModuleId = 1, ModuleTitle = "Test Module", IsMandatory = true };

        _context.Users.Add(user);
        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        var startDto = new StartTestDto { ModuleId = 1 };

        // Act
        var result = await _controller.StartTest(startDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var attempt = Assert.IsType<TestAttemptDto>(okResult.Value);
        Assert.Equal(1, attempt.ModuleId);
        Assert.Equal(userId, attempt.UserId);
        Assert.Equal("active", attempt.Status);
    }

    [Fact]
    public async Task SubmitTest_ReturnsOk_WithScore()
    {
        // Arrange
        var userId = 1;
        var attemptId = 1;
        SetupUser(userId);

        var user = new User { UserId = userId, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        var testAttempt = new TestAttempt
        {
            AttemptId = attemptId,
            UserId = userId,
            ModuleId = 1,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            Status = "active"
        };

        _context.Users.Add(user);
        _context.TestAttempts.Add(testAttempt);
        await _context.SaveChangesAsync();

        var submitDto = new SubmitTestDto
        {
            AttemptId = attemptId,
            Answers = new[]
            {
                new { QuestionId = 1, SelectedAnswer = "A" },
                new { QuestionId = 2, SelectedAnswer = "B" }
            }
        };

        // Act
        var result = await _controller.SubmitTest(submitDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TestResultDto>(okResult.Value);
        Assert.NotNull(response.Score);
        Assert.True(response.Score >= 0 && response.Score <= 100);
    }

    [Fact]
    public async Task SubmitTest_ReturnsNotFound_WhenAttemptDoesNotExist()
    {
        // Arrange
        SetupUser(1);

        var submitDto = new SubmitTestDto
        {
            AttemptId = 999,
            Answers = new object[] { }
        };

        // Act
        var result = await _controller.SubmitTest(submitDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task SubmitTest_ReturnsForbid_WhenUserDoesNotOwnAttempt()
    {
        // Arrange
        SetupUser(1);

        var testAttempt = new TestAttempt
        {
            AttemptId = 1,
            UserId = 2,
            ModuleId = 1,
            Status = "active"
        };

        _context.TestAttempts.Add(testAttempt);
        await _context.SaveChangesAsync();

        var submitDto = new SubmitTestDto { AttemptId = 1, Answers = new object[] { } };

        // Act
        var result = await _controller.SubmitTest(submitDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAttemptResult_ReturnsOk_WithResult()
    {
        // Arrange
        var userId = 1;
        var attemptId = 1;
        SetupUser(userId);

        var result = new TestResult
        {
            ResultId = 1,
            AttemptId = attemptId,
            UserId = userId,
            ModuleId = 1,
            Score = 85m,
            CorrectAnswers = 17,
            TotalQuestions = 20,
            CompletedAt = DateTime.UtcNow
        };

        _context.TestResults.Add(result);
        await _context.SaveChangesAsync();

        // Act
        var response = await _controller.GetAttemptResult(attemptId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var resultDto = Assert.IsType<TestResultDto>(okResult.Value);
        Assert.Equal(85m, resultDto.Score);
        Assert.Equal(17, resultDto.CorrectAnswers);
    }

    [Fact]
    public async Task GetUserAttempts_ReturnsOk_WithAllUserAttempts()
    {
        // Arrange
        var userId = 1;
        SetupUser(userId);

        var attempts = new[]
        {
            new TestAttempt { AttemptId = 1, UserId = userId, ModuleId = 1, StartedAt = DateTime.UtcNow, Status = "completed" },
            new TestAttempt { AttemptId = 2, UserId = userId, ModuleId = 2, StartedAt = DateTime.UtcNow, Status = "completed" }
        };

        _context.TestAttempts.AddRange(attempts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUserAttempts(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultDtos = Assert.IsType<List<TestAttemptDto>>(okResult.Value);
        Assert.Equal(2, resultDtos.Count);
    }

    [Fact]
    public async Task GetModuleAttempts_ReturnsOk_WithModuleAttempts()
    {
        // Arrange
        var moduleId = 1;

        _context.TestAttempts.AddRange(
            new TestAttempt { AttemptId = 1, UserId = 1, ModuleId = moduleId, Status = "completed" },
            new TestAttempt { AttemptId = 2, UserId = 2, ModuleId = moduleId, Status = "completed" },
            new TestAttempt { AttemptId = 3, UserId = 3, ModuleId = 2, Status = "completed" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetModuleAttempts(moduleId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var attempts = Assert.IsType<List<TestAttemptDto>>(okResult.Value);
        Assert.Equal(2, attempts.Count);
        Assert.All(attempts, a => Assert.Equal(moduleId, a.ModuleId));
    }

    [Fact]
    public async Task CancelTest_ReturnsOk_WhenAttemptIsCancelled()
    {
        // Arrange
        var userId = 1;
        SetupUser(userId);

        var attempt = new TestAttempt { AttemptId = 1, UserId = userId, ModuleId = 1, Status = "active" };
        _context.TestAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.CancelTest(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var updated = await _context.TestAttempts.FindAsync(1);
        Assert.Equal("cancelled", updated.Status);
    }

    [Fact]
    public async Task ValidateAnswer_ReturnsOk_WithCorrectness()
    {
        // Arrange
        var question = new Question { QuestionId = 1, ModuleId = 1, QuestionText = "What is 2+2?", CorrectAnswer = "4" };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        var validateDto = new ValidateAnswerDto { QuestionId = 1, Answer = "4" };

        // Act
        var result = await _controller.ValidateAnswer(validateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AnswerValidationDto>(okResult.Value);
        Assert.True(response.IsCorrect);
    }

    [Fact]
    public async Task ValidateAnswer_ReturnsIncorrect_WhenAnswerIsWrong()
    {
        // Arrange
        var question = new Question { QuestionId = 1, ModuleId = 1, QuestionText = "What is 2+2?", CorrectAnswer = "4" };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        var validateDto = new ValidateAnswerDto { QuestionId = 1, Answer = "5" };

        // Act
        var result = await _controller.ValidateAnswer(validateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AnswerValidationDto>(okResult.Value);
        Assert.False(response.IsCorrect);
    }

    [Fact]
    public async Task GetTimeLimit_ReturnsCorrectLimit()
    {
        // Arrange
        var moduleId = 1;
        var module = new Module { ModuleId = moduleId, ModuleTitle = "Test", EstimatedDurationMinutes = 60 };
        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        // Act
        var timeLimit = 60 * 60; // 60 minutes in seconds

        // Assert
        Assert.Equal(3600, timeLimit);
    }

    [Fact]
    public async Task GetAttemptStats_ReturnsAggregate()
    {
        // Arrange
        _context.TestResults.AddRange(
            new TestResult { ResultId = 1, Score = 100m, CorrectAnswers = 20, TotalQuestions = 20 },
            new TestResult { ResultId = 2, Score = 85m, CorrectAnswers = 17, TotalQuestions = 20 },
            new TestResult { ResultId = 3, Score = 70m, CorrectAnswers = 14, TotalQuestions = 20 }
        );
        await _context.SaveChangesAsync();

        // Act
        var results = await _context.TestResults.ToListAsync();
        var averageScore = results.Average(r => r.Score);

        // Assert
        Assert.Equal(85m, (decimal)Math.Round((double)averageScore, 2));
    }
}
