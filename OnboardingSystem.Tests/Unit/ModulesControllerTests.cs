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

public class ModulesControllerTests
{
    private readonly Mock<ILogger<ModulesController>> _loggerMock;
    private readonly AppDbContext _context;
    private readonly ModulesController _controller;

    public ModulesControllerTests()
    {
        _loggerMock = new Mock<ILogger<ModulesController>>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new ModulesController(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithAllModules()
    {
        // Arrange
        var modules = new[]
        {
            new Module { ModuleId = 1, ModuleTitle = "Модуль 1", Description = "Описание 1", IsMandatory = true },
            new Module { ModuleId = 2, ModuleTitle = "Модуль 2", Description = "Описание 2", IsMandatory = false }
        };

        _context.Modules.AddRange(modules);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedModules = Assert.IsType<List<ModuleDto>>(okResult.Value);
        Assert.Equal(2, returnedModules.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenModuleExists()
    {
        // Arrange
        var module = new Module
        {
            ModuleId = 1,
            ModuleTitle = "Тестовый модуль",
            Description = "Описание",
            IsMandatory = true,
            Content = "Содержание"
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedModule = Assert.IsType<ModuleDto>(okResult.Value);
        Assert.Equal("Тестовый модуль", returnedModule.ModuleTitle);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenModuleDoesNotExist()
    {
        // Act
        var result = await _controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithValidData()
    {
        // Arrange
        var moduleDto = new CreateModuleDto
        {
            ModuleTitle = "Новый модуль",
            Description = "Описание",
            IsMandatory = true,
            Content = "Содержание",
            EstimatedDurationMinutes = 120
        };

        // Act
        var result = await _controller.Create(moduleDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedModule = Assert.IsType<ModuleDto>(createdResult.Value);
        Assert.Equal("Новый модуль", returnedModule.ModuleTitle);

        // Verify in database
        var moduleInDb = await _context.Modules.FirstOrDefaultAsync(m => m.ModuleTitle == "Новый модуль");
        Assert.NotNull(moduleInDb);
    }

    [Fact]
    public async Task Update_ReturnsOk_WithValidData()
    {
        // Arrange
        var module = new Module
        {
            ModuleId = 1,
            ModuleTitle = "Старое название",
            Description = "Описание",
            IsMandatory = true
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateModuleDto
        {
            ModuleTitle = "Новое название",
            Description = "Новое описание",
            IsMandatory = false
        };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var updatedModule = await _context.Modules.FindAsync(1);
        Assert.Equal("Новое название", updatedModule.ModuleTitle);
        Assert.Equal("Новое описание", updatedModule.Description);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenModuleExists()
    {
        // Arrange
        var module = new Module { ModuleId = 1, ModuleTitle = "Удаляемый модуль" };
        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);

        var deletedModule = await _context.Modules.FindAsync(1);
        Assert.Null(deletedModule);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenModuleDoesNotExist()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetByDepartment_ReturnsOk_WithModulesForDepartment()
    {
        // Arrange
        var module = new Module
        {
            ModuleId = 1,
            ModuleTitle = "Модуль",
            DepartmentId = 5
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByDepartment(5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var modules = Assert.IsType<List<ModuleDto>>(okResult.Value);
        Assert.Single(modules);
    }

    [Fact]
    public async Task GetMandatory_ReturnsOnlyMandatoryModules()
    {
        // Arrange
        _context.Modules.AddRange(
            new Module { ModuleId = 1, ModuleTitle = "Обязательный 1", IsMandatory = true },
            new Module { ModuleId = 2, ModuleTitle = "Дополнительный", IsMandatory = false },
            new Module { ModuleId = 3, ModuleTitle = "Обязательный 2", IsMandatory = true }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetMandatory();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var modules = Assert.IsType<List<ModuleDto>>(okResult.Value);
        Assert.Equal(2, modules.Count);
        Assert.All(modules, m => Assert.True(m.IsMandatory));
    }
}
