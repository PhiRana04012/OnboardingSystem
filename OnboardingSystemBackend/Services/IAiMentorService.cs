namespace OnboardingSystem.Services;

public interface IAiMentorService
{
    /// <param name="includeKnowledgeContext">false — без PDF/FAQ в system prompt (аналитика, планы)</param>
    Task<string> AskAsync(
        string message,
        bool includeKnowledgeContext = true,
        CancellationToken cancellationToken = default);
}
