using Microsoft.AspNetCore.Mvc;
using OnboardingSystem.DTOs;
using OnboardingSystem.Services;

namespace OnboardingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AiMentorController : ControllerBase
{
    private readonly IAiMentorService _aiMentorService;
    private readonly ILogger<AiMentorController> _logger;

    public AiMentorController(IAiMentorService aiMentorService, ILogger<AiMentorController> logger)
    {
        _aiMentorService = aiMentorService;
        _logger = logger;
    }

    [HttpPost("chat")]
    [ProducesResponseType(typeof(AiMentorChatResponseDto), 200)]
    [ProducesResponseType(typeof(AiMentorChatResponseDto), 400)]
    [ProducesResponseType(typeof(AiMentorChatResponseDto), 500)]
    public async Task<ActionResult<AiMentorChatResponseDto>> Chat([FromBody] AiMentorChatRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new AiMentorChatResponseDto
            {
                Success = false,
                ErrorMessage = "Введите вопрос для AI-наставника."
            });
        }

        try
        {
            var reply = await _aiMentorService.AskAsync(
                request.Message,
                includeKnowledgeContext: true,
                cancellationToken);
            return Ok(new AiMentorChatResponseDto
            {
                Success = true,
                Reply = reply
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AiMentor chat failed");
            return StatusCode(500, new AiMentorChatResponseDto
            {
                Success = false,
                ErrorMessage = "Не удалось получить ответ AI-наставника."
            });
        }
    }
}
