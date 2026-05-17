using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JobTitlesController : ControllerBase
{
    private readonly AppDbContext _context;

    public JobTitlesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список всех должностей
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<JobTitleDto>), 200)]
    public async Task<ActionResult<List<JobTitleDto>>> GetJobTitles()
    {
        var jobTitles = await _context.JobTitles
            .Select(jt => new JobTitleDto
            {
                JobTitleId = jt.JobTitleId,
                Title = jt.Title,
                Description = jt.Description
            })
            .ToListAsync();

        return Ok(jobTitles);
    }

    /// <summary>
    /// Получить должность по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JobTitleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<JobTitleDto>> GetJobTitle(int id)
    {
        var jobTitle = await _context.JobTitles
            .Where(jt => jt.JobTitleId == id)
            .Select(jt => new JobTitleDto
            {
                JobTitleId = jt.JobTitleId,
                Title = jt.Title,
                Description = jt.Description
            })
            .FirstOrDefaultAsync();

        if (jobTitle == null)
        {
            return NotFound();
        }

        return Ok(jobTitle);
    }
}
