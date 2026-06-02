using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OnboardingSystem.Data;
using OnboardingSystem.Entities;
using OnboardingSystem.Hubs;
using OnboardingSystem.Services;
using Xunit;

namespace OnboardingSystem.Tests.Unit;

public class NotificationServiceTests
{
    private readonly AppDbContext _context;
    private readonly NotificationService _service;
    private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;

    public NotificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _hubContextMock = new Mock<IHubContext<NotificationHub>>();
        _hubClientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();

        _hubClientsMock
            .Setup(x => x.Group(It.IsAny<string>()))
            .Returns(_clientProxyMock.Object);

        _hubContextMock
            .Setup(x => x.Clients)
            .Returns(_hubClientsMock.Object);

        _clientProxyMock
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new NotificationService(_context, _hubContextMock.Object);
    }

    [Fact]
    public async Task SendAsync_CreatesNotification_AndSendsToHub()
    {
        var user = new User { UserId = 1, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.SendAsync(1, "info", "Test", "Message", "/link");

        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
        Assert.Equal("Message", result.Message);

        var saved = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == result.NotificationId);
        Assert.NotNull(saved);

        _hubClientsMock.Verify(x => x.Group(It.IsAny<string>()), Times.Once);
        _clientProxyMock.Verify(x => x.SendCoreAsync("ReceiveNotification", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_CreatesNotificationWithoutLinkUrl()
    {
        var user = new User { UserId = 1, Email = "user@test.com", FullName = "User", OnboardingStatus = "В процессе" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.SendAsync(1, "info", "Test", "Message");

        Assert.NotNull(result);
        Assert.Null(result.LinkUrl);
    }
}
