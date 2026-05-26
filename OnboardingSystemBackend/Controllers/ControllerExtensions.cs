using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OnboardingSystem.Data;
using OnboardingSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnboardingSystem.Controllers;

/// <summary>
/// Extension методы для контроллеров
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Получить текущего пользователя из JWT Claims
    /// </summary>
    public static async Task<User?> GetCurrentUserAsync(this ControllerBase controller, AppDbContext context)
    {
        var userIdClaim = controller.User.FindFirst("sub");
        if (!int.TryParse(userIdClaim?.Value, out var userId))
        {
            return null;
        }

        return await context.Users
            .Include(u => u.Roles)
            .Include(u => u.Department)
            .Include(u => u.JobTitle)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    /// <summary>
    /// Получить ID текущего пользователя из JWT Claims
    /// </summary>
    public static int? GetCurrentUserId(this ControllerBase controller)
    {
        var userIdClaim = controller.User.FindFirst("sub");
        if (int.TryParse(userIdClaim?.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}
