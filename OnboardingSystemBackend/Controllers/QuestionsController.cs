using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;
using OnboardingSystem.DTOs;
using OnboardingSystem.Entities;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QuestionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(AppDbContext context, ILogger<QuestionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить вопросы по модулю
    /// </summary>
    [HttpGet("module/{moduleId}")]
    [ProducesResponseType(typeof(List<QuestionDto>), 200)]
    public async Task<ActionResult<List<QuestionDto>>> GetQuestionsByModule(int moduleId)
    {
        var questions = await _context.Questions
            .Include(q => q.AnswerOptions)
            .Where(q => q.ModuleId == moduleId)
            .Select(q => new QuestionDto
            {
                QuestionId = q.QuestionId,
                ModuleId = q.ModuleId,
                QuestionText = q.QuestionText,
                AnswerOptions = q.AnswerOptions.Select(a => new AnswerOptionDto
                {
                    AnswerId = a.AnswerId,
                    QuestionId = a.QuestionId,
                    AnswerText = a.AnswerText,
                    IsCorrect = a.IsCorrect
                }).ToList()
            })
            .ToListAsync();

        return Ok(questions);
    }

    /// <summary>
    /// Получить вопросы для теста (без правильных ответов)
    /// </summary>
    [HttpGet("module/{moduleId}/test")]
    [ProducesResponseType(typeof(List<QuestionForTestDto>), 200)]
    public async Task<ActionResult<List<QuestionForTestDto>>> GetQuestionsForTest(int moduleId)
    {
        var module = await _context.Modules
            .Include(m => m.Questions)
            .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(m => m.ModuleId == moduleId);

        if (module == null)
        {
            return NotFound("Модуль не найден");
        }

        if (module.Questions.Count < 10)
        {
            return BadRequest("В модуле должно быть минимум 10 вопросов");
        }

        // Случайный выбор вопросов (минимум 10)
        var random = new Random();
        var selectedQuestions = module.Questions
            .OrderBy(x => random.Next())
            .Take(Math.Max(10, module.Questions.Count))
            .Select(q => new QuestionForTestDto
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                AnswerOptions = q.AnswerOptions.Select(a => new AnswerOptionForTestDto
                {
                    AnswerId = a.AnswerId,
                    AnswerText = a.AnswerText
                }).OrderBy(x => random.Next()).ToList()
            })
            .ToList();

        return Ok(selectedQuestions);
    }

    /// <summary>
    /// Получить вопрос по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QuestionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<QuestionDto>> GetQuestion(int id)
    {
        var question = await _context.Questions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.QuestionId == id);

        if (question == null)
        {
            return NotFound();
        }

        var questionDto = new QuestionDto
        {
            QuestionId = question.QuestionId,
            ModuleId = question.ModuleId,
            QuestionText = question.QuestionText,
            AnswerOptions = question.AnswerOptions.Select(a => new AnswerOptionDto
            {
                AnswerId = a.AnswerId,
                QuestionId = a.QuestionId,
                AnswerText = a.AnswerText,
                IsCorrect = a.IsCorrect
            }).ToList()
        };

        return Ok(questionDto);
    }

    /// <summary>
    /// Создать вопрос
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(QuestionDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<QuestionDto>> CreateQuestion([FromBody] CreateQuestionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var module = await _context.Modules.FindAsync(dto.ModuleId);
        if (module == null)
        {
            return BadRequest("Модуль не найден");
        }

        if (dto.AnswerOptions.Count < 2)
        {
            return BadRequest("Вопрос должен содержать минимум 2 варианта ответа");
        }

        if (dto.AnswerOptions.Count(a => a.IsCorrect) != 1)
        {
            return BadRequest("Должен быть ровно один правильный ответ");
        }

        var question = new Question
        {
            ModuleId = dto.ModuleId,
            QuestionText = dto.QuestionText,
            AnswerOptions = dto.AnswerOptions.Select(a => new AnswerOption
            {
                AnswerText = a.AnswerText,
                IsCorrect = a.IsCorrect
            }).ToList()
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        await _context.Entry(question)
            .Collection(q => q.AnswerOptions)
            .LoadAsync();

        var questionDto = new QuestionDto
        {
            QuestionId = question.QuestionId,
            ModuleId = question.ModuleId,
            QuestionText = question.QuestionText,
            AnswerOptions = question.AnswerOptions.Select(a => new AnswerOptionDto
            {
                AnswerId = a.AnswerId,
                QuestionId = a.QuestionId,
                AnswerText = a.AnswerText,
                IsCorrect = a.IsCorrect
            }).ToList()
        };

        return CreatedAtAction(nameof(GetQuestion), new { id = question.QuestionId }, questionDto);
    }

    /// <summary>
    /// Обновить вопрос
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(QuestionDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<QuestionDto>> UpdateQuestion(int id, [FromBody] UpdateQuestionDto dto)
    {
        var question = await _context.Questions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.QuestionId == id);

        if (question == null)
        {
            return NotFound();
        }

        if (dto.QuestionText != null)
        {
            question.QuestionText = dto.QuestionText;
        }

        if (dto.AnswerOptions != null)
        {
            if (dto.AnswerOptions.Count < 2)
            {
                return BadRequest("Вопрос должен содержать минимум 2 варианта ответа");
            }

            if (dto.AnswerOptions.Count(a => a.IsCorrect == true) != 1)
            {
                return BadRequest("Должен быть ровно один правильный ответ");
            }

            // Удаляем старые варианты ответов
            _context.AnswerOptions.RemoveRange(question.AnswerOptions);

            // Добавляем новые
            question.AnswerOptions = dto.AnswerOptions.Select(a => new AnswerOption
            {
                AnswerText = a.AnswerText!,
                IsCorrect = a.IsCorrect ?? false
            }).ToList();
        }

        await _context.SaveChangesAsync();

        await _context.Entry(question)
            .Collection(q => q.AnswerOptions)
            .LoadAsync();

        var questionDto = new QuestionDto
        {
            QuestionId = question.QuestionId,
            ModuleId = question.ModuleId,
            QuestionText = question.QuestionText,
            AnswerOptions = question.AnswerOptions.Select(a => new AnswerOptionDto
            {
                AnswerId = a.AnswerId,
                QuestionId = a.QuestionId,
                AnswerText = a.AnswerText,
                IsCorrect = a.IsCorrect
            }).ToList()
        };

        return Ok(questionDto);
    }

    /// <summary>
    /// Удалить вопрос
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
        {
            return NotFound();
        }

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


