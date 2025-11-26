namespace OnboardingSystem.DTOs;

public class TestAttemptDto
{
    public long AttemptId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = null!;
    public DateTime AttemptDate { get; set; }
    public int AttemptNumber { get; set; }
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
}

public class SubmitTestDto
{
    public int UserId { get; set; }
    public int ModuleId { get; set; }
    public List<TestAnswerDto> Answers { get; set; } = new();
}

public class TestAnswerDto
{
    public int QuestionId { get; set; }
    public int AnswerId { get; set; }
}

public class TestResultDto
{
    public long AttemptId { get; set; }
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = null!;
    public DateTime AttemptDate { get; set; }
    public int AttemptNumber { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
    public bool CanRetry { get; set; }
    public int RemainingAttempts { get; set; }
    public List<QuestionResultDto> QuestionResults { get; set; } = new();
}

public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public int? SelectedAnswerId { get; set; }
    public int? CorrectAnswerId { get; set; }
    public bool IsCorrect { get; set; }
}

