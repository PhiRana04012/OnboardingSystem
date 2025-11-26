namespace OnboardingSystem.DTOs;

public class QuestionDto
{
    public int QuestionId { get; set; }
    public int ModuleId { get; set; }
    public string QuestionText { get; set; } = null!;
    public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
}

public class QuestionForTestDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public List<AnswerOptionForTestDto> AnswerOptions { get; set; } = new();
}

public class AnswerOptionDto
{
    public int AnswerId { get; set; }
    public int QuestionId { get; set; }
    public string AnswerText { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

public class AnswerOptionForTestDto
{
    public int AnswerId { get; set; }
    public string AnswerText { get; set; } = null!;
}

public class CreateQuestionDto
{
    public int ModuleId { get; set; }
    public string QuestionText { get; set; } = null!;
    public List<CreateAnswerOptionDto> AnswerOptions { get; set; } = new();
}

public class CreateAnswerOptionDto
{
    public string AnswerText { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

public class UpdateQuestionDto
{
    public string? QuestionText { get; set; }
    public List<UpdateAnswerOptionDto>? AnswerOptions { get; set; }
}

public class UpdateAnswerOptionDto
{
    public int? AnswerId { get; set; }
    public string? AnswerText { get; set; }
    public bool? IsCorrect { get; set; }
}


