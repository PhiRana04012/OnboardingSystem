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

public class ModulesControllerTests
{
    private readonly Mock<ILogger<ModulesController>> _loggerMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly AppDbContext _context;
    private readonly ModulesController _controller;

    public ModulesControllerTests()
    {
        _loggerMock = new Mock<ILogger<ModulesController>>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new ModulesController(_context, _loggerMock.Object, _authorizationServiceMock.Object);

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "1") }, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };

        _authorizationServiceMock.Setup(x => x.IsAdmin(It.IsAny<User>())).Returns(true);
        _authorizationServiceMock.Setup(x => x.IsHr(It.IsAny<User>())).Returns(false);
        _authorizationServiceMock.Setup(x => x.IsDepartmentHead(It.IsAny<User>())).Returns(false);
    }

    [Fact]
    public async Task GetModules_ReturnsOk_WithAllModules()
    {
        var modules = new[]
        {
            new Module { ModuleId = 1, Title = "Module 1", Description = "Description 1", IsMandatory = true },
            new Module { ModuleId = 2, Title = "Module 2", Description = "Description 2", IsMandatory = false }
        };

        _context.Modules.AddRange(modules);
        await _context.SaveChangesAsync();

        var result = await _controller.GetModules(null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedModules = Assert.IsType<List<ModuleDto>>(okResult.Value);
        Assert.Equal(2, returnedModules.Count);
    }

    [Fact]
    public async Task GetModules_ReturnsOk_FilteredByMandatory()
    {
        _context.Modules.AddRange(
            new Module { ModuleId = 1, Title = "Module 1", IsMandatory = true },
            new Module { ModuleId = 2, Title = "Module 2", IsMandatory = false },
            new Module { ModuleId = 3, Title = "Module 3", IsMandatory = true }
        );
        await _context.SaveChangesAsync();

        var result = await _controller.GetModules(null, true);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedModules = Assert.IsType<List<ModuleDto>>(okResult.Value);
        Assert.Equal(2, returnedModules.Count);
    }

    [Fact]
    public async Task GetModule_ReturnsOk_WhenModuleExists()
    {
        var module = new Module
        {
            ModuleId = 1,
            Title = "Test Module",
            Description = "Test Description",
            IsMandatory = true,
            Content = "Test Content"
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        var result = await _controller.GetModule(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedModule = Assert.IsType<ModuleDto>(okResult.Value);
        Assert.Equal("Test Module", returnedModule.Title);
    }

    [Fact]
    public async Task GetModule_ReturnsNotFound_WhenModuleDoesNotExist()
    {
        var result = await _controller.GetModule(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateModule_ReturnsCreated()
    {
        var moduleDto = new CreateModuleDto
        {
            Title = "New Module",
            Description = "New Description",
            IsMandatory = true,
            Content = "New Content"
        };

        var result = await _controller.CreateModule(moduleDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedModule = Assert.IsType<ModuleDto>(createdResult.Value);
        Assert.Equal("New Module", returnedModule.Title);
    }

    [Fact]
    public async Task UpdateModule_ReturnsOk()
    {
        var module = new Module
        {
            ModuleId = 1,
            Title = "Old Title",
            Description = "Old Description",
            IsMandatory = true
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateModuleDto
        {
            Title = "New Title",
            Description = "New Description",
            IsMandatory = false
        };

        var result = await _controller.UpdateModule(1, updateDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var updatedModule = await _context.Modules.FindAsync(1);
        Assert.Equal("New Title", updatedModule.Title);
    }

    [Fact]
    public async Task DeleteModule_ReturnsNoContent()
    {
        var module = new Module { ModuleId = 1, Title = "To Delete" };
        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteModule(1);

        Assert.IsType<NoContentResult>(result);

        var deletedModule = await _context.Modules.FindAsync(1);
        Assert.Null(deletedModule);
    }

    [Fact]
    public async Task DeleteModule_ReturnsNotFound_WhenModuleDoesNotExist()
    {
        var result = await _controller.DeleteModule(999);
        Assert.IsType<NotFoundResult>(result);
    }
}
