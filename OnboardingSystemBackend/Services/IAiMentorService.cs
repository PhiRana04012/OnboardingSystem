namespace OnboardingSystem.Services;

public interface IAiMentorService
{
    Task<string> AskAsync(string message, CancellationToken cancellationToken = default);
}
