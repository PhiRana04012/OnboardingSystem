using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
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
    public async Task SendAsync_CreatesNotification_AndReturnsDto()
    {
        var dto = await _service.SendAsync(42, "info", "Привет", "Сообщение", "/link");

        Assert.Equal("info", dto.Type);
        Assert.Equal("Привет", dto.Title);
        Assert.Equal("Сообщение", dto.Message);
        Assert.Equal("/link", dto.LinkUrl);
        Assert.False(dto.IsRead);
        Assert.True(dto.CreatedAt <= DateTime.UtcNow);

        var persisted = await _context.Notifications.SingleAsync();
        Assert.Equal(dto.NotificationId, persisted.NotificationId);
        Assert.Equal(1, await _service.GetUnreadCountAsync(42));
    }

    [Fact]
    public async Task MarkAsReadAsync_MarksNotificationRead()
    {
        var dto = await _service.SendAsync(55, "info", "Заголовок", "Сообщение");

        var result = await _service.MarkAsReadAsync(dto.NotificationId, 55);

        Assert.True(result);
        Assert.Equal(0, await _service.GetUnreadCountAsync(55));
    }

    [Fact]
    public async Task MarkAllAsReadAsync_AllNotificationsBecomeRead()
    {
        await _service.SendAsync(66, "info", "A", "1");
        await _service.SendAsync(66, "info", "B", "2");

        await _service.MarkAllAsReadAsync(66);

        Assert.Equal(0, await _service.GetUnreadCountAsync(66));
        var list = await _service.GetRecentAsync(66);
        Assert.All(list, item => Assert.True(item.IsRead));
    }
}
