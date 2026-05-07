namespace OnboardingSystem.DTOs;

public class AiMentorChatRequestDto
{
    public string Message { get; set; } = string.Empty;
}

public class AiMentorChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
