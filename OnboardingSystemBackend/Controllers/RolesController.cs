using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список всех ролей
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .Select(r => new { r.RoleId, r.RoleName })
            .ToListAsync();

        return Ok(roles);
    }
}
