using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OnboardingSystem.Controllers;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;
using OnboardingSystem.Services;
using Xunit;

namespace OnboardingSystem.Tests.Unit;

public class UsersControllerTests
{
    private readonly AppDbContext _context;
    private readonly UsersController _controller;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IAuthenticationProvider> _authProviderMock;
    private readonly PasswordHasher _passwordHasher;
    private readonly Mock<IPasswordResetService> _passwordResetServiceMock;
    private readonly Mock<IAppAuthorizationService> _authorizationServiceMock;

    public UsersControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _emailServiceMock = new Mock<IEmailService>();
        _authProviderMock = new Mock<IAuthenticationProvider>();
        _passwordHasher = new PasswordHasher();
        _passwordResetServiceMock = new Mock<IPasswordResetService>();
        _authorizationServiceMock = new Mock<IAppAuthorizationService>();

        _passwordResetServiceMock
            .Setup(x => x.CreatePasswordSetupTokenAsync(It.IsAny<int>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync("setup-token");

        _emailServiceMock
            .Setup(x => x.SendPasswordSetupEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _authorizationServiceMock
            .Setup(x => x.CanCreateOrDeleteUser(It.IsAny<User>()))
            .Returns(true);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Frontend:BaseUrl"] = "https://app.local"
            })
            .Build();

        _controller = new UsersController(
            _context,
            Mock.Of<ILogger<UsersController>>(),
            _emailServiceMock.Object,
            _authProviderMock.Object,
            _passwordHasher,
            _passwordResetServiceMock.Object,
            configuration,
            _authorizationServiceMock.Object);
    }

    [Fact]
    public async Task CreateUser_ReturnsCreated_WhenUserEmailIsUnique()
    {
        var dto = new CreateUserDto
        {
            FullName = "Новый пользователь",
            Email = "newuser@example.com",
            DepartmentId = 1,
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
            RoleIds = new List<int>()
        };

        var result = await _controller.CreateUser(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(UsersController.GetUser), createdResult.ActionName);
        Assert.Equal(1, await _context.Users.CountAsync());
    }

    [Fact]
    public async Task CreateUser_ReturnsConflict_WhenEmailAlreadyExists()
    {
        _context.Users.Add(new User
        {
            Email = "existing@example.com",
            FullName = "Существующий пользователь"
        });
        await _context.SaveChangesAsync();

        var dto = new CreateUserDto
        {
            FullName = "Другой пользователь",
            Email = "existing@example.com",
            DepartmentId = 1,
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
            RoleIds = new List<int>()
        };

        var result = await _controller.CreateUser(dto);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }
}
